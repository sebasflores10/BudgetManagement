using AutoMapper;
using BudgetManagement.Models;
using Microsoft.Data.SqlClient.Diagnostics;

namespace BudgetManagement.Services
{
    /// <summary>
    /// (Udemy): 131. Utilizando AutoMapper
    /// </summary>
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// Constructor 'AutoMapperProfiles'
        /// </summary>
        public AutoMapperProfiles()
        {
            CreateMap<BudgetAccount, BudgetAccountCreateViewModel>();
            CreateMap<UpdateTransactionsViewModel, Transactions>().ReverseMap();
        }
    }
}
