﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Visual Studio via: 
//     Edit > Paste Special > Paste JSON as Classes
//     
//     JSON generated from: /DefaultCollection/TestProject1/_apis/build/builds/1
//
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace Inedo.Extensions.TFS.VisualStudioOnline.Model
{
    public class GetBuildResponse
    {
        public _Links _links { get; set; }
        public int id { get; set; }
        public string url { get; set; }
        public Definition definition { get; set; }
        public string buildNumber { get; set; }
        public int buildNumberRevision { get; set; }
        public Project1 project { get; set; }
        public string uri { get; set; }
        public string sourceBranch { get; set; }
        public string sourceVersion { get; set; }
        public string status { get; set; }
        public Queue queue { get; set; }
        public DateTime queueTime { get; set; }
        public string priority { get; set; }
        public DateTime startTime { get; set; }
        public DateTime finishTime { get; set; }
        public string reason { get; set; }
        public string result { get; set; }
        public Requestedfor requestedFor { get; set; }
        public Requestedby requestedBy { get; set; }
        public DateTime lastChangedDate { get; set; }
        public Lastchangedby lastChangedBy { get; set; }
        public string parameters { get; set; }
        public Orchestrationplan orchestrationPlan { get; set; }
        public Logs logs { get; set; }
        public Repository repository { get; set; }
        public bool keepForever { get; set; }
    }
}
