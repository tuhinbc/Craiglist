using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

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

        public Form1()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            //MessageBox.Show(args.Length.ToString());
            if (args.Length != 4)
            {
                log_book("command line parameters not given!");
                Environment.Exit(0);
            }
            user = args[1];
            password = args[2];
            post_id = args[3];


            login();
        }
        void log_book(string message)
        {
            File.AppendAllText("log.txt",DateTime.Now.ToString()+" > "+ message+Environment.NewLine);
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
                //webBrowser1.Document.Cookie = "";
                //webBrowser1.Navigate("javascript:void((function(){var a,b,c,e,f;f=0;a=document.cookie.split('; ');for(e=0;e<a.length&&a[e];e++){f++;for(b='.'+location.host;b;b=b.replace(/^(?:%5C.|[^%5C.]+)/,'')){for(c=location.pathname;c;c=c.replace(/.$/,'')){document.cookie=(a[e]+'; domain='+b+'; path='+c+'; expires='+new Date((new Date()).getTime()-1e11).toGMTString());}}}})())");
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
                    Environment.Exit(10);
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
                HtmlElementCollection cc;
                string tmp = "";
                title = webBrowser1.Document.GetElementById("PostingTitle").GetAttribute("value");

                tmp = webBrowser1.Document.GetElementsByTagName("header")[0].InnerText;

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
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
            }
            else if (current_task == task.repostContinueImages)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.publish;
                if (debug) write_log("continue repost images");
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("button");
                foreach (HtmlElement ht in cc)
                {
                    if (debug) write_log(ht.GetAttribute("value"));
                    if (ht.GetAttribute("type") != "submit") continue;
                    if (ht.GetAttribute("value").Contains("done with images"))
                    //if (ht.GetAttribute("value").Contains("Images"))
                    {
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
                }
            }
            else if (current_task == task.publish)
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                current_task = task.postStatus;
                if (debug) write_log("publish click");
                HtmlElementCollection cc = webBrowser1.Document.GetElementsByTagName("button");
                foreach (HtmlElement ht in cc)
                {
                    if (debug) write_log(ht.GetAttribute("value"));
                    if (ht.GetAttribute("type") != "submit") continue;
                    //if (ht.GetAttribute("value").Contains("Done with Images"))
                    //if (ht.GetAttribute("value").Contains("Continue"))
                    if (ht.GetAttribute("value").Contains("publish"))
                    {
                        ht.InvokeMember("click");
                        //write_log(ht.GetAttribute("value"));
                        break;
                    }
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
                    log_book("(E-Mail Verification required) reposting succeded.username:" + user + " password:" + password + " area & category:" + a_c + " Post ID:" + post_id);
                }
                else
                {
                    if (debug) write_log("Posted");
                    log_book(" REPOSTING SUCCEDED.USERNAME:" + user + " PASSWORD:" + password + " TITLE:" + title + " AREA & CATEGORY:" + a_c + " POST ID:" + post_id);
                }
                Environment.Exit(0);
            }
        }
    }
}
