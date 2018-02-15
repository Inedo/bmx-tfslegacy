﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Visual Studio via: 
//     Edit > Paste Special > Paste JSON as Classes
//     
//     JSON generated from: /DefaultCollection/_apis/wit/workitemtypes?ids=3,4&fields=System.Id,System.Title,System.Description,System.State,System.CreatedDate,System.CreatedBy,System.WorkItemType&api-version=1.0
//
// </auto-generated>
//------------------------------------------------------------------------------

namespace Inedo.Extensions.TFS.VisualStudioOnline.Model
{
    public class GetWorkItemTypesResponse
    {
        public int count { get; set; }
        public GetWorkItemTypeResponse[] value { get; set; }
    }

    public class GetWorkItemTypeResponse
    {
        public string name { get; set; }
        public string description { get; set; }
        //public string xmlForm { get; set; } // uninclused because result is very large and unfilterable
        //public Fieldinstance[] fieldInstances { get; set; } // uninclused because result is very large and unfilterable
        public Transitions transitions { get; set; }
        public string url { get; set; }
    }

    public class Transitions
    {
        public Transition[] Approved { get; set; }
        public Transition[] Committed { get; set; }
        public Transition[] Done { get; set; }
        public Transition[] New { get; set; }
        public Transition[] _ { get; set; }
        public Transition[] Removed { get; set; }
        public Transition[] Requested { get; set; }
        public Transition[] Accepted { get; set; }
        public Transition[] Closed { get; set; }
        public Transition[] InProgress { get; set; }
        public Transition[] ToDo { get; set; }
        public Transition[] Active { get; set; }
        public Transition[] Open { get; set; }
        public Transition[] Design { get; set; }
        public Transition[] Ready { get; set; }
        public Transition[] Inactive { get; set; }
        public Transition[] Completed { get; set; }
        public Transition[] InPlanning { get; set; }
    }

    public class Transition
    {
        public string to { get; set; }
    }
}
