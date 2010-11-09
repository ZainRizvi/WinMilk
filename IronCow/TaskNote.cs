﻿using System;
using System.ComponentModel;
using IronCow.Rest;

namespace IronCow
{
    public class TaskNote : RtmFatElement, INotifyPropertyChanged
    {
        #region Public Properties
        public DateTime Created { get; private set; }
        public DateTime? Modified { get; private set; }

        private string mTitle = string.Empty;
        public string Title
        {
            get { return mTitle; }
            set
            {
                mTitle = value ?? string.Empty;
                Upload();
                OnPropertyChanged("Title");
            }
        }

        private string mBody = string.Empty;
        public string Body
        {
            get { return mBody; }
            set
            {
                mBody = value ?? string.Empty;
                Upload();
                OnPropertyChanged("Body");
            }
        }

        public DateTime CreatedOrModified
        {
            get
            {
                return Modified ?? Created;
            }
        }
        #endregion

        #region Construction
        public TaskNote()
        {
        }

        public TaskNote(string title)
        {
            Title = title;
        }

        public TaskNote(string title, string body)
            : this(title)
        {
            Body = body;
        }

        internal TaskNote(RawNote note)
        {
            Sync(note);
        }
        #endregion

        #region Syncing Methods
        protected override void DoSync(bool firstSync, RawRtmElement element)
        {
            base.DoSync(firstSync, element);

            RawNote note = (RawNote)element;
            Created = DateTime.Parse(note.Created);
            if (!string.IsNullOrEmpty(note.Modified))
                Modified = DateTime.Parse(note.Modified);

            mTitle = note.Title ?? string.Empty;
            mBody = note.Body ?? string.Empty;

            OnPropertyChanged("Title");
            OnPropertyChanged("Body");
        }

        protected override void OnSyncingChanged()
        {
            base.OnSyncingChanged();
        }

        private void Upload()
        {
            if (Syncing && IsSynced)
            {
                RestRequest request = new RestRequest("rtm.tasks.notes.edit");
                request.Parameters.Add("timeline", Owner.GetTimeline().ToString());
                request.Parameters.Add("note_id", Id.ToString());
                request.Parameters.Add("note_title", Title);
                request.Parameters.Add("note_text", Body);
                Owner.ExecuteRequest(request);
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                if (Rtm.Dispatcher != null && !Rtm.Dispatcher.CheckAccess())
                    Rtm.Dispatcher.BeginInvoke(new Action(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName))));
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
