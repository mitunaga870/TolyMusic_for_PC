﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TolyMusic_for_PC
{
    public class Track
    {
        public string Composer_id;
        public string Group_id;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Title_pron { get; set; }
        public double Duration { get; set; }
        public int TrackNumber { get; set; }

        public Track(Dictionary<string,object> dictionary)
        {
            Composer_id = dictionary["composer_id"].ToString();
            Group_id = dictionary["group_id"].ToString();
            Id = dictionary["track_id"].ToString();
            Title = dictionary["track_title"].ToString();
            Path = dictionary["path"].ToString();
            Title_pron = dictionary["track_title_pron"].ToString();
            Duration = (double) dictionary["duration"];
            if(dictionary["track_num"] != DBNull.Value)
                TrackNumber = (int)dictionary["track_num"];
            else
                TrackNumber = -1;
        }
    }
}