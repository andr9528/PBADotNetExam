using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Service.Banking.Domain.Concrete;
using Service.Banking.Domain.Core;
using Shared.Repository.Core;

namespace Service.Banking.Repository.EntityFramework
{
    /// <summary>  
    ///  This class should be accessed via the Generic or Serializable classes that inherit from it.  
    /// </summary>   
    public abstract class BaseEntityRepositoryHandler : IBaseRepository
    {
        internal EntityRepository repo = null;
        private bool _useLazyLoading = false;
        public BaseEntityRepositoryHandler(EntityRepository repo)
        {
            this.repo = repo;
        }

        public void ResetRepo()
        {
            throw new NotImplementedException();

            //repo.Dispose();
            //repo = null;

            //repo = new EntityRepository(_useLazyLoading);
        }


        public void Save()
        {
            repo.SaveChanges();
        }

        #region Help Methods
        internal EntityState CheckEntryState(EntityState state, EntityEntry entry)
        {
            if (entry != null)
                state = entry.State;
            return state;
        }

        internal bool VerifyEntryState(EntityState actualState, EntityState desiredState)
        {
            return actualState == desiredState ? true : false;
        }

        internal string GetAmountAdded(ICollection<bool> results)
        {
            return string.Format("Added {0} out of {1}.", results.Where(b => b).Count(), results.Count);
        }
        #endregion

        #region Find Query Builders

        // There should be one query for each case in either 'Find' or 'FindMultiple',
        // meaning if there is a case to find YourDomainClass in both methods,
        // then there should be one query builder, meant to build queries for YourDomainClass
        // as both find methods make use of the same query, only the amount of elements returned are diffent.
        /*
        private IQueryable<YourDomainClass> BuildFindYourDomainClassQuery(IYourDomainClass y, IQueryable<YourDomainClass> query)
        {
            Check whether or not a property has been set, if it has been set, add a where to the query including the property.

            // e.g
            if (y.PropertyA != default(PropertyAType))
                query = query.Where(x => x.PropertyA == y.PropertyA);

            // If it is a string then use the method 'IsNullOrEmpty' and the method 'Contains'

            if (!y.PropertyB.IsNullOrEmpty())
                query = query.Where(x => x.PropertyB.Contains(y.PropertyB));
            return query;
        }
        */

        private IQueryable<Person> BuildFindPersonQuery(IPerson p, IQueryable<Person> query)
        {
            if (!p.PersonNumber.IsNullOrEmpty())
                query = query.Where(x => x.PersonNumber.Contains(p.PersonNumber));
            if (!p.Name.IsNullOrEmpty())
                query = query.Where(x => x.Name.Contains(p.Name));
            if (!p.Address.IsNullOrEmpty())
                query = query.Where(x => x.Address.Contains(p.Address));

            if (p.Id != default(int))
                query = query.Where(x => x.Id == p.Id);

            return query;
        }

        private IQueryable<Account> BuildFindAccountQuery(IAccount a, IQueryable<Account> query)
        {
            if (!a.AccountNumber.IsNullOrEmpty())
                query = query.Where(x => x.AccountNumber.Contains(a.AccountNumber));

            if (a.Balance!= default(double))
                query = query.Where(x => x.Balance == a.Balance);

            if (a.Id != default(int))
                query = query.Where(x => x.Id == a.Id);

            if (a.Owner != null)
            {
                if (!a.Owner.PersonNumber.IsNullOrEmpty())
                    query = query.Where(x => x.Owner. PersonNumber.Contains(a.Owner.PersonNumber));
                if (!a.Owner.Name.IsNullOrEmpty())
                    query = query.Where(x => x.Owner.Name.Contains(a.Owner.Name));
                if (!a.Owner.Address.IsNullOrEmpty())
                    query = query.Where(x => x.Owner.Address.Contains(a.Owner.Address));

                if (a.Id != default(int))
                    query = query.Where(x => x.Owner.Id == a.Owner.Id);
            }

            return query;
        }
        #endregion

        #region Find Multiple Methods

        private ICollection<T> FindMultipleResults<T>(IQueryable<T> query) where T : class, IEntity
        {
            var result = query.ToList().Distinct();
            if (result.Count() > 0)
                return new List<T>(result);
            else
                throw new Exception(string.Format("Found no results for {0}", typeof(T).Name));
        }
        // Create methods for all the different classes, where you should be able to get multiple specific elements.

        // e.g
        /*
        internal ICollection<YourDomainClass> FindMultipleYourDomainClass(IYourDomainClass y)
        {
            var query = repo.YourDomainClassInPlural.AsQueryable();
            query = BuildFindYourDomainClassQuery(y, query);

            return FindMultipleResults(query);
        }
        */
        internal ICollection<Person> FindMultiplePeople(IPerson p)
        {
            var query = repo.Persons.Include(x => x.Accounts).AsQueryable();
            query = BuildFindPersonQuery(p, query);

            return FindMultipleResults(query);
        }

        internal ICollection<Account> FindMultipleAccounts(IAccount a)
        {
            var query = repo.Accounts.Include(x => x.Owner).AsQueryable();
            query = BuildFindAccountQuery(a, query);

            return FindMultipleResults(query);
        }



        #endregion

        #region Find Single Methods

        private T FindAResult<T>(IQueryable<T> query) where T : class, IEntity
        {
            var result = query.ToList().Distinct();
            if (result.Count() == 1)
                return result.First();
            else if (result.Count() > 1)
                throw new Exception(string.Format("More than 1 result found when searching for a {0}", typeof(T).Name));
            else
                throw new Exception(string.Format("No results found when searching for a {0}", typeof(T).Name));
        }
        // Create methods for all the different classes, where you should be able to get one specific element.

        // e.g
        /*
        internal IYourDomainClass FindYourDomainClass(IYourDomainClass y)
        {
            var query = repo.YourDomainClassAsPlural.AsQueryable();
            query = BuildFindYourDomainClassQuery(y, query);

            return FindAResult(query);
        }
        */

        #endregion

        #region Add Methods

        // There should be one method for each case in the switch on the Generic version, or each overload in the Serializable version

        // e.g
        /*
        internal bool AddYourDomainClass(IYourDomainClass y)
        {
            EntityEntry entry = null;
            EntityState state = EntityState.Unchanged;
            
            entry = repo.Add(y);

            state = CheckEntryState(state, entry);
            return VerifyEntryState(state, EntityState.Added);
        }        
         */

        internal bool AddPerson(IPerson p)
        {
            EntityEntry entry = null;
            EntityState state = EntityState.Unchanged;

            entry = repo.Add(p);

            state = CheckEntryState(state, entry);
            return VerifyEntryState(state, EntityState.Added);
        }

        internal bool AddAccount(IAccount a)
        {
            EntityEntry entry = null;
            EntityState state = EntityState.Unchanged;

            entry = repo.Add(a);

            state = CheckEntryState(state, entry);
            return VerifyEntryState(state, EntityState.Added);
        }
        #endregion

        #region Update Methods

        // There should be one method for each case in the switch on the Generic version, or each overload in the Serializable version

        // e.g
        /*
        internal bool UpdateYourDomainClass(IYourDomainClass y)
        {
            EntityEntry entry = null;
            EntityState state = EntityState.Unchanged;
            
            entry = repo.Update(y);

            state = CheckEntryState(state, entry);
            return VerifyEntryState(state, EntityState.Modified);
        }        
         */

        #endregion

        #region Delete Methods

        // There should be one method for each case in the switch on the Generic version, or each overload in the Serializable version

        // e.g
        /*
        internal bool DeleteYourDomainClass(IYourDomainClass y)
        {
            EntityEntry entry = null;
            EntityState state = EntityState.Unchanged;
            
            entry = repo.Remove(y);

            state = CheckEntryState(state, entry);
            return VerifyEntryState(state, EntityState.Deleted);
        }        
         */

        #endregion
    }
}
