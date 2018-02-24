﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "Polkovnik.DroidInjector.FodySample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);
            Injector.InjectViews();
        }
        
        private void InjectTest(View view)
        {
            var v = view.FindViewById(Resource.Id.myButton);
            var b = v == null;
            if (b)
            {
                throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                ((Button)v).Click += ButtonClick;
                ((Button)v).LongClick += ButtonClick;
                ((TextView)v).TextChanged += ButtonClick;
            }

            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                //throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                v.Click += ButtonClick2;
            }
            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                //throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                v.Click += ButtonClick3;
            }
            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                v.Click += ButtonClick4;
            }
            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                v.Click += ButtonClick5;
            }
            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                ((Button)v).Click += ButtonClick6;
            }
            v = view.FindViewById(Resource.Id.myButton);
            b = v == null;
            if (b)
            {
                //throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                ((Button)v).Click += ButtonClick7;
            }
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick(object sender, EventArgs args)
        {
            
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick2(object sender, EventArgs args)
        {

        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick3(object sender, EventArgs args)
        {

        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick4(object sender, EventArgs args)
        {

        }


        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick5(object sender, EventArgs args)
        {

        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick6(object sender, EventArgs args)
        {

        }

        [ViewEvent(Resource.Id.myButton, typeof(TextView), nameof(TextView.TextChanged))]
        private void ButtonClick7(object sender, EventArgs args)
        {

        }
    }
}

