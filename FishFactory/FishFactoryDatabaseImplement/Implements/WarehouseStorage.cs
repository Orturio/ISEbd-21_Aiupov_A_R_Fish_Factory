﻿using System;
using FishFactoryBusinessLogic.Interfaces;
using FishFactoryBusinessLogic.ViewModels;
using FishFactoryBusinessLogic.BindingModels;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FishFactoryDatabaseImplement.Models;

namespace FishFactoryDatabaseImplement.Implements
{
    public class WarehouseStorage : IWarehouseStorage
    {
        public List<WarehouseViewModel> GetFullList()
        {
            using (var context = new FishFactoryDatabase())
            {
                return context.Warehouses
                .Include(rec => rec.WarehouseComponent)
                .ThenInclude(rec => rec.Component)
                .ToList()
                .Select(rec => new WarehouseViewModel
                {
                    Id = rec.Id,
                    WarehouseName = rec.WarehouseName,
                    Responsible = rec.Responsible,
                    DateCreate = rec.DateCreate,
                    WarehouseComponents = rec.WarehouseComponent
                .ToDictionary(recPC => recPC.ComponentId, recPC =>
                (recPC.Component?.ComponentName, recPC.Count))
                })
                .ToList();
            }
        }

        public List<WarehouseViewModel> GetFilteredList(WarehouseBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new FishFactoryDatabase())
            {
                return context.Warehouses
                .Include(rec => rec.WarehouseComponent)
                .ThenInclude(rec => rec.Component)
                .Where(rec => rec.WarehouseName.Contains(model.WarehouseName))
                .ToList()
                .Select(rec => new WarehouseViewModel
                {
                    Id = rec.Id,
                    WarehouseName = rec.WarehouseName,
                    Responsible = rec.Responsible,
                    DateCreate = rec.DateCreate,
                    WarehouseComponents = rec.WarehouseComponent
                .ToDictionary(recPC => recPC.ComponentId, recPC =>
                (recPC.Component?.ComponentName, recPC.Count))
                })
                .ToList();
            }
        }

        public WarehouseViewModel GetElement(WarehouseBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new FishFactoryDatabase())
            {
                var warehouse = context.Warehouses
                .Include(rec => rec.WarehouseComponent)
                .ThenInclude(rec => rec.Component)
                .FirstOrDefault(rec => rec.WarehouseName.Equals(model.WarehouseName) || rec.Id
                == model.Id);
                return warehouse != null ?
                new WarehouseViewModel
                {
                    Id = warehouse.Id,
                    WarehouseName = warehouse.WarehouseName,
                    Responsible = warehouse.Responsible,
                    DateCreate = warehouse.DateCreate,
                    WarehouseComponents = warehouse.WarehouseComponent
                .ToDictionary(recPC => recPC.ComponentId, recPC =>
                (recPC.Component?.ComponentName, recPC.Count))
                } : null;
            }
        }

        public void Insert(WarehouseBindingModel model)
        {
            using (var context = new FishFactoryDatabase())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Warehouse warehouse = CreateModel(model, new Warehouse());
                        context.Warehouses.Add(warehouse);
                        context.SaveChanges();
                        CreateModel(model, warehouse, context);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Update(WarehouseBindingModel model)
        {
            using (var context = new FishFactoryDatabase())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var element = context.Warehouses.FirstOrDefault(rec => rec.Id == model.Id);
                        if (element == null)
                        {
                            throw new Exception("Элемент не найден");
                        }
                        CreateModel(model, element, context);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Delete(WarehouseBindingModel model)
        {
            using (var context = new FishFactoryDatabase())
            {
                Warehouse element = context.Warehouses.FirstOrDefault(rec => rec.Id == model.Id);
                if (element != null)
                {
                    context.Warehouses.Remove(element);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Элемент не найден");
                }
            }
        }

        private Warehouse CreateModel(WarehouseBindingModel model, Warehouse warehouse)
        {
            warehouse.WarehouseName = model.WarehouseName;
            warehouse.Responsible = model.Responsible;
            warehouse.DateCreate = model.DateCreate;
            return warehouse;
        }

        private Warehouse CreateModel(WarehouseBindingModel model, Warehouse warehouse,
        FishFactoryDatabase context)
        {
            warehouse.WarehouseName = model.WarehouseName;
            warehouse.Responsible = model.Responsible;
            warehouse.DateCreate = model.DateCreate;
            if (model.Id.HasValue)
            {
                var productComponents = context.WarehouseComponents.Where(rec =>
                rec.WarehouseId == model.Id.Value).ToList();
                // удалили те, которых нет в модели
                context.WarehouseComponents.RemoveRange(productComponents.Where(rec =>
                !model.WarehouseComponents.ContainsKey(rec.ComponentId)).ToList());
                context.SaveChanges();
                // обновили количество у существующих записей
                foreach (var updateComponent in productComponents)
                {
                    updateComponent.Count =
                    model.WarehouseComponents[updateComponent.ComponentId].Item2;
                    model.WarehouseComponents.Remove(updateComponent.ComponentId);
                }
                context.SaveChanges();
            }
            // добавили новые
            foreach (var pc in model.WarehouseComponents)
            {
                context.WarehouseComponents.Add(new WarehouseComponent
                {
                    WarehouseId = warehouse.Id,
                    ComponentId = pc.Key,
                    Count = pc.Value.Item2,
                });
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    throw;
                }
            }
            return warehouse;
        }

        public bool Unrestocking(int Count, int CannedId)
        {
            using (var context = new FishFactoryDatabase())
            {
                var list = GetFullList();
                var DCount = context.CannedComponents.Where(rec => rec.CannedId == CannedId)
                    .ToDictionary(rec => rec.ComponentId, rec => rec.Count * Count);

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var key in DCount.Keys.ToArray())
                        {
                            foreach (var warehouseComponent in context.WarehouseComponents.Where(rec => rec.ComponentId == key))
                            {
                                if (warehouseComponent.Count > DCount[key])
                                {
                                    warehouseComponent.Count -= DCount[key];
                                    DCount[key] = 0;
                                    break;
                                }
                                else
                                {
                                    DCount[key] -= warehouseComponent.Count;
                                    warehouseComponent.Count = 0;
                                }
                            }
                            if (DCount[key] > 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        context.SaveChanges();
                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}