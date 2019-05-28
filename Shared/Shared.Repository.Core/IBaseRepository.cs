using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Repository.Core
{
    public interface IBaseRepository
    {
        /*
         * Class that implemnts this should hold all the code that does the work with interacting with the chosen repository framework.
         * Doing this, the Generic or Serializable only has to inherit from the base one, and then call the correct methods on the base.
         */

        void Save();
        void ResetRepo();
    }
}
