﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Repository.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Domain.Core;

namespace Main.Repository.EntityFramework
{
    public class GenericEntityRepositoryHandler : BaseEntityRepositoryHandler, IGenericRepository
    {
        public GenericEntityRepositoryHandler(EntityRepository repository) : base(repository)
        {

        }

        bool IGenericRepository.Add<T>(T element)
        {
            bool result = false;

            switch (element)
            {
                // Create cases for all the different classes that should be addable to the database.
                // Remember to setup Uniqueness for columns that are not allowed to contain duplicates

                // Example:
                // case IYourDomainClass y:
                //    result = AddYourDomainClass(y);
                //    break;
                case IEvent e:
                    result = AddEvent(e);
                    break;
                case IRollbackData r:
                    result = AddRollbackData(r);
                    break;
                default:
                    throw new Exception("ERROR ERROR ERROR");
            }

            return result;
        }

        string IGenericRepository.AddMultiple<T>(ICollection<T> elements)
        {
            List<bool> results = new List<bool>();

            foreach (var element in elements)
            {
                results.Add((this as IGenericRepository).Add(element));
            }

            return GetAmountAdded(results);
        }

        bool IGenericRepository.Delete<T>(T element)
        {
            if (element.Id == 0)
                throw new Exception(string.Format("I need an Id to figure out what to remove"), new ArgumentException("Id of predicate can't be 0"));
            bool result = false;

            switch (element)
            {
                //Create cases for all the different classes that should be updateable in the database.

                // Example:
                // case IYourDomainClass y:
                //    result = DeleteYourDomainClass(y);
                //    break;
                case IEvent e:
                    result = DeleteEvent(e);
                    break;
                default:
                    throw new Exception("ERROR ERROR ERROR");
            }
			
			return result;
        }

        T IGenericRepository.Find<T>(T predicate)
        {
            IEntity entity = null;
            switch (predicate)
            {
                // Create cases for all the different classes that should be retriable from the database

                // Example:
                // case IYourDomainClass y:
                //    entity = FindYourDomainClass(y);
                //    break;
                default:
                    throw new Exception("ERROR ERROR ERROR");
            }

            return entity as T;
        }

        ICollection<T> IGenericRepository.FindMultiple<T>(T predicate)
        {
            ICollection<T> entities = null;
            switch (predicate)
            {
                // Create cases for all the different classes that should be retrievable from the database

                // Example:
                // case IYourDomainClass y:
                //    entities = FindMultipleYourDomainClassInPlural(y) as ICollection<T>;
                //    break;
                case IEvent e:
                    entities = FindMultipleEvents(e) as ICollection<T>;
                    break;
                case IRollbackData r:
                    entities = FindMultipleRollbackDatas(r) as ICollection<T>;
                    break;
                default:
                    throw new Exception("ERROR ERROR ERROR");
            }

            return entities;
        }



        bool IGenericRepository.Update<T>(T element)
        {
            if (element.Id == 0)
                throw new Exception(string.Format("I need an Id to figure out what to update"), new ArgumentException("Id of predicate can not be 0"));
            bool result = false;

            switch (element)
            {
                //Create cases for all the differnet classes that should be updateable in the database.

                // Example:
                // case IYourDomainClass y:
                //    result = UpdateYourDomainClass(y);
                //    break;
                case IEvent e:
                    result = UpdateEvent(e);
                    break;
                case IRollbackData r:
                    result = UpdateRollbackData(r);
                    break;
                default:
                    throw new Exception("ERROR ERROR ERROR");
            }
            
            return result;
        }
    }
}
