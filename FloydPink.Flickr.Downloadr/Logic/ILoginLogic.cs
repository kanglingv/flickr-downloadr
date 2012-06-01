﻿using System;
using FloydPink.Flickr.Downloadr.Model;

namespace FloydPink.Flickr.Downloadr.Logic
{
    public interface ILoginLogic
    {
        void Login(Action<User> applyUser);
        void Logout();
        bool IsUserLoggedIn(Action<User> applyUser);
    }
}