﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WinMilk.RTM
{
    public enum TaskListSortOrder { Priority, Date, Name }

    [DataContract]
    public class TaskList : IComparable, INotifyPropertyChanged
    {
        private ObservableCollection<Task> _tasks;

        [DataMember]
        public ObservableCollection<Task> Tasks
        {
            get { return _tasks; }
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    NotifyPropertyChanged("Tasks");
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("HasItems");

                    if (Tasks != null)
                        Tasks.CollectionChanged += 
                            new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Tasks_CollectionChanged);
                }
            }
        }

        public string Count
        {
            get
            {
                if (IsSmart) return "smart";
                if (Tasks == null) return "";
                return Tasks.Count.ToString();
            }
        }

        public bool HasItems
        {
            get
            {
                return Tasks != null && Tasks.Count != 0;
            }
        }

        [DataMember]
        public int Id { get; set; }

        private bool _isSmart;
        [DataMember]
        public bool IsSmart
        {
            get
            {
                return _isSmart;
            }
            set
            {
                _isSmart = value;
                NotifyPropertyChanged("IsSmart");
                NotifyPropertyChanged("Count");
            }
        }
        public bool IsNormal { get { return !IsSmart; } }

        [DataMember]
        public string Filter { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public TaskListSortOrder SortOrder { get; set; }

        public TaskList(int id, string name, bool isSmart, string filter, TaskListSortOrder sortOrder)
        {
            Id = id;
            Name = name;
            IsSmart = isSmart;
            Filter = filter;
            SortOrder = sortOrder;
        }

        void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Tasks");
            NotifyPropertyChanged("HasItems");
        }

        public TaskList()
            : this(0, "", false, "", 0)
        { }

        public static TaskListSortOrder ParseSortOrder(string str)
        {
            if (str == "0") return TaskListSortOrder.Priority;
            if (str == "1") return TaskListSortOrder.Date;
            return TaskListSortOrder.Name;
        }

        public int CompareTo(object obj)
        {
            TaskList other = obj as TaskList;

            if (this.Name == "Inbox") return -1;
            if (other.Name == "Inbox") return 1;
            if (this.Name == "Sent") return 1;
            if (other.Name == "Sent") return -1;

            if (this.IsSmart != other.IsSmart)
            {
                if (this.IsSmart) return 1;
                else return -1;
            }
            else
            {
                return this.Name.CompareTo(other.Name);
            }
        }


        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
