﻿using Shared.Repository.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Ordering.Repository.EntityFramework
{
    public class SerializableEntityRepositoryHandler : BaseEntityRepositoryHandler, ISerializableRepository
    {
        public SerializableEntityRepositoryHandler(EntityRepository repository) : base(repository)
        {

        }

        // Below an Example of implementing one domain this way is shown.


        // e.g 
        /*

        public IYourDomainClass Find(IYourDomainClass predicate)
        {
            return FindYourDomainClass(predicate)
        }
            
        public ICollection<IYourDomainClass> FindMultiple(IYourDomainClass predicate)
        {
            return FindMultipleYourDomainClass(predicate)
        }
            
        public bool Update(IYourDomainClass element)
        {
            return UpdateYourDomainClass(element)
        }
            
        public bool Delete(IYourDomainClass element)
        {
            return DeleteYourDomainClass(element)
        }
            
        public bool Add(IYourDomainClass element)
        {
            return AddYourDomainClass(element)
        }

            
        public string AddMultiple(ICollection<IYourDomainClass> elements)
        {
            List<bool> results = new List<bool>();

            foreach (var element in elements)
            {
                results.Add(Add(element));
            }

            return GetAmountAdded(results);
        }

         */
    }
}
