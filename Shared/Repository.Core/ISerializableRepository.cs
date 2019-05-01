using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Repository.Core
{
    public interface ISerializableRepository : IBaseRepository
    {
        /*
         * Classes that implement this class should also inherit from a class implementing IBaseRepository
         *
         * Make exactly one Find, FindMultiple, Update, Delete, Add and AddMultiple, for each domain object that implements IEntity
         * Doing this creates overloads for the methods.
         * The Methods call should call the respective methods on the class that implent IBaseRepository
         *
         * Remember to always call Save after Adding, Updateing and Deleteing.
         * Also Remember to call ResetRepo after Updateing and Deleteing.
         *
         * e.g
         *
         * IYourDomainClass Find(IYourDomainClass predicate);
         *
         * ICollection<IYourDomainClass> FindMultiple(IYourDomainClass predicate);
         *
         * bool Update(IYourDomainClass element);
         *
         * bool Delete(IYourDomainClass element);
         *
         * bool Add(IYourDomainClass element);
         *
         * string AddMultiple(ICollection<IYourDomainClass> elements);
         *
         */
    }
}
