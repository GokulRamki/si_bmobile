//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace si_bmobile.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class favorite_plans
    {
        public long Id { get; set; }
        public System.DateTime date { get; set; }
        public Nullable<long> user { get; set; }
        public Nullable<long> denom_id { get; set; }
        public Nullable<long> plan_id { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public Nullable<long> plan_type_id { get; set; }
    
        public virtual plan_denomination plan_denomination { get; set; }
        public virtual registration registration { get; set; }
    }
}