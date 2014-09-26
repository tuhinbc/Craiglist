using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace craiglist
{
    public partial class Form1 : Form
    {
        enum task : int
        {
            removeCookie,
            putLoginCredential,
            goToPost,
            searchForRepost,
            doRepost,
            repostContinue,
            repostContinueImages,
            publish,
            postStatus
        }
        string site = "https://accounts.craigslist.org/login";
        task current_task;

        ///EDIT THOSE
        //string target = "Edit";
        string target = "Repost";

        string user = "";
        string password = "";
        string post_id = "";
        string title = "";
        string a_c = "";

        ///EDIT THOSE

        bool login_success = false;
        bool debug = true;
        bool debug_view = false;

        public Form1()
        {
            InitializeComponent();
            
            string[] args = Environment.GetCommandLineArgs();
            //MessageBox.Show(args.Length.ToString());
            if (args.Length < 4)
            {
                log_book("Command line parameters not given!");
                Environment.Exit(0);
            }
            if (args.Length == 5)
            {
                if (args[4] == "debug") debug_view = true;
                this.Opacity = 100;
                this.ShowInTaskbar = true;
            }
            user = args[1];
            password = args[2];
            post_id = args[3];
            if (!debug_view) DisableClickSounds();

            login();
        }
        /// <summary>
        /// disable ie sound
        /// </summary>
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(
                FEATURE_DISABLE_NAVIGATION_SOUNDS,
                SET_FEATURE_ON_PROCESS,
                true);
        }

        /// END
        /// 
        void log_book(string message)
        {
            string[] args = Environment.GetCommandLineArgs();
            string msg;
            msg = DateTime.Now.ToString() + " > " + message + " - PARAMETERS: ";
            for (int i = 1; i < args.Length; i++)
            {
                msg += args[i] + " ";
            }
            if (title.Length > 0) msg += " - TITLE:" + title;
            if (a_c.Length > 0) msg += " - AREA & CATEGORY:" + a_c;

            File.AppendAllText(Application.StartupPath + "\\log.txt", msg + Environment.NewLine);
        }
        void write_log(string message)
        {
            richTextBox1.AppendText("--" + message + Environment.NewLine);
            //writer.WriteLine(message);
            //writer.Flush();
        }
        public void login()
        {
            
            //debug=false;
            current_task = task.putLoginCredential;
            //current_task = task.removeCookie;
            //webBrowser1.
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(site);
            
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }
        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (debug) write_log("task is ="+(current_task.ToString()));
            //webBrowser1.Stop();
            if (current_task == task.removeCookie)
            {
                write_log("removing cookie");
                current_task = task.putLoginCredential;
                webBrowser1.Navigate(site);
            }
            else if (current_task == task.putLoginCredential)
            {
                
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("a");
                foreach (HtmlElement ht in cc)
                {
                    if (ht.GetAttribute("href").Contains("logout"))
                    {
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        return;
                    }
                }
                
                if (debug) write_log("entering login data");
                //webBrowser1.
                //while (webBrowser1.ReadyState != WebBrowserReadyState.Complete);
                current_task = task.goToPost;
                webBrowser1.Document.GetElementById("inputEmailHandle").SetAttribute("value", user);
                webBrowser1.Document.GetElementById("inputPassword").SetAttribute("value", password);
                webBrowser1.Document.GetElementsByTagName("button")[0].InvokeMember("click");

            }
            else if (current_task == task.goToPost)
            {
                if (debug) write_log("checking log stat");
                //while (webBrowser1.ReadyState != WebBrowserReadyState.Complete);
                current_task = task.searchForRepost;

                HtmlElement ht = webBrowser1.Document.GetElementsByTagName("a")[0];
                if (ht.GetAttribute("href").Contains("logout")) login_success = true;
                if (!login_success)
                {
                    log_book("Can't log in. Check username/password.");
                    if(!debug_view)
                    Environment.Exit(1);
                }
                if (debug) write_log("going to post");
                webBrowser1.Navigate("https://post.craigslist.org/manage/" + post_id);

            }
            else if (current_task == task.searchForRepost)
            {
                bool has_repost = false;
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.repostContinue;
                if (debug) write_log("searching for repost");
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement ht in cc)
                {
                    if (ht.GetAttribute("type") != "submit") continue;
                    if (ht.GetAttribute("value").Contains(target))
                    {
                        has_repost = true;
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
                if (!has_repost)
                {
                    log_book("Given post id has no option for reposting.");
                    if (!debug_view) Environment.Exit(10);
                }
                //write_log("Test");
            }
            else if (current_task == task.repostContinue)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.repostContinueImages;
                if (debug) write_log("continue repost");

                ///retrive posting data
                ///
                bool has_ok = false;
                HtmlElementCollection cc;
                string tmp = "";
                title = webBrowser1.Document.GetElementById("PostingTitle").GetAttribute("value");

                tmp = webBrowser1.Document.GetElementsByTagName("header")[0].InnerText;
                tmp = tmp.Substring(tmp.IndexOf("logout") + 8);
                tmp = tmp.Substring(0, tmp.Length - 14);
                if (debug) write_log(tmp);
                a_c = tmp;
                //write_log(tmp.Substring(start,end-start+1));
                //MessageBox.Show(tmp);
                ///
                cc = webBrowser1.Document.GetElementsByTagName("button");
                foreach (HtmlElement ht in cc)
                {
                    //write_log(ht.GetAttribute("value"));
                    if (ht.GetAttribute("type") != "submit") continue;
                    //if (ht.GetAttribute("value").Contains("Continue"))
                    if (ht.GetAttribute("value").Contains("continue"))
                    {
                        has_ok = true;
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
                if (!has_ok)
                {
                    log_book("Srver error may occured(clicking continue).try again.");
                    if (!debug_view) Environment.Exit(10);
                }
            }
            else if (current_task == task.repostContinueImages)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.publish;
                if (debug) write_log("continue repost images");
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("button");
                bool has_ok = false;
                foreach (HtmlElement ht in cc)
                {
                    if (debug) write_log(ht.GetAttribute("value"));
                    if (ht.GetAttribute("type") != "submit") continue;
                    if (ht.GetAttribute("value").Contains("done with images"))
                    //if (ht.GetAttribute("value").Contains("Images"))
                    {
                        has_ok = true;
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
                if (!has_ok)
                {
                    if (debug) write_log("continue with images not found.searching for publish button.");

                    HtmlElementCollection cc2 = webBrowser1.Document.GetElementsByTagName("button");
                    foreach (HtmlElement ht in cc2)
                    {
                        if (debug) write_log(ht.GetAttribute("value"));
                        if (ht.GetAttribute("type") != "submit") continue;
                        //if (ht.GetAttribute("value").Contains("Done with Images"))
                        //if (ht.GetAttribute("value").Contains("Continue"))
                        if (ht.GetAttribute("value").Contains("publish"))
                        {
                            has_ok = true;
                            current_task = task.postStatus;
                            ht.InvokeMember("click");
                            //write_log(ht.GetAttribute("value"));
                            break;
                        }
                    }
                    if (!has_ok)
                    {
                        log_book("Srver error may occured(clicking (done with images+clicking publish)).try again.");
                        if (!debug_view) Environment.Exit(10);
                    }
                }
            }
            else if (current_task == task.publish)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.postStatus;
                if (debug) write_log("publish click");
                bool has_ok = false;
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("button");
                foreach (HtmlElement ht in cc)
                {
                    if (debug) write_log(ht.GetAttribute("value"));
                    if (ht.GetAttribute("type") != "submit") continue;
                    //if (ht.GetAttribute("value").Contains("Done with Images"))
                    //if (ht.GetAttribute("value").Contains("Continue"))
                    if (ht.GetAttribute("value").Contains("publish"))
                    {
                        has_ok = true;
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
                if (!has_ok)
                {
                    log_book("Srver error may occured(clicking publish).try again.");
                    if (!debug_view) Environment.Exit(10);
                }
            }
            else if (current_task == task.postStatus)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                if (debug) write_log("post status");
                HtmlDocument doc = webBrowser1.Document;
                if (debug) write_log(doc.ToString());
                //MessageBox.Show(doc.Body.InnerText);
                if (doc.Body.InnerText.Contains("with a link to"))
                {
                    if (debug) write_log("verification needed");
                    log_book("(E-MAIL VERIFICATION REQUIRED) REPOSTING SUCCEEDED");
                }
                else //if(doc.Body.InnerText.Contains("your post at"))
                {
                    if (debug) write_log("Posted");
                    log_book(" REPOSTING SUCCEEDED");
                }
                    /*
                else
                {
                    log_book(" FAILED(SERVER OR OTHER ERROR OCCURED).USERNAME:" + user + " PASSWORD:" + password + " TITLE:" + title + " AREA & CATEGORY:" + a_c + " POST ID:" + post_id);
                }*/
                if (!debug_view)
                Environment.Exit(0);
            }
        }
    }
}
