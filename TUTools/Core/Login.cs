using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Auth0.OidcClient;
using static TUTools.Properties.Settings;
using System.Diagnostics;

namespace TUTools.Core
{
    public class Login : GH_Component
    {
        public List<string> log = new List<string>();
        public bool loggedIn = false;

        public Login()
          : base("Login", "Login",
              "Login to TU Tools using a DIT email address.",
              "TU Tools", "Core")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run the login application.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Information log.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;

            if (!DA.GetData(0, ref run)) return;

            // Set up our client to handle the login.
            Auth0ClientOptions clientOptions = new Auth0ClientOptions
            {
                Domain = "axisarch.eu.auth0.com",
                ClientId = "7xAlz55NWm8pl2ZAEvEXsh4UtRpm8fuq",
                Browser = new WebBrowserBrowser("Authenticating...", 400, 640)
            };

            // Initiate the client
            var client = new Auth0Client(clientOptions);
            clientOptions.PostLogoutRedirectUri = clientOptions.RedirectUri;

            var extra = new Dictionary<string, string>()
            {
                {"response_type", "code"}
            };

            // Handle the logout.
            if (loggedIn && !run)
            {
                client.LogoutAsync();
                loggedIn = false;
                this.Message = "Logged Out";
                log.Add("Logged out of Axis at " + System.DateTime.Now.ToShortDateString());
            }

            // Handle the login.
            if (!loggedIn && run)
            {
                client.LoginAsync(extra).ContinueWith(t =>
                {
                    if (!t.Result.IsError)
                    {
                        Default.Token = t.Result.AccessToken;
                        log.Clear();

                        log.Add("Logged in to Axis at " + DateTime.Now.ToShortTimeString());
                        DateTime validTo = DateTime.Now.AddDays(3);
                        log.Add("Login valid to: " + validTo.ToLongDateString() + ", " + validTo.ToShortTimeString());

                        // Update our login time.
                        Default.ValidTo = validTo;
                        this.Message = "Logged In";
                        loggedIn = true;
                    }
                    else
                    {
                        Debug.WriteLine("Error logging in: " + t.Result.Error);
                        log.Add(t.Result.ToString());
                        log.Add("Error logging in: " + t.Result.Error);
                        loggedIn = false;
                    }
                    t.Dispose();
                });
            }
            Default.LoggedIn = loggedIn;
            DA.SetDataList(0, log);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Login;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e3f4b1a8-439b-4d40-80ec-dfb5ee1452fc"); }
        }
    }
}