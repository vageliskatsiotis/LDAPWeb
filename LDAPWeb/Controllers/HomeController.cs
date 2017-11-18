using System;
using System.Web.Mvc;
using Novell.Directory.Ldap;
using System.Collections;
using System.Threading;

namespace LDAPWeb.Controllers
{
    public class HomeController : Controller
    {
        // Public Variables
        public string ldapHost = "localhost";
        public int ldapPort = 10389;
        public string adminUname = "uid=admin,ou=system";
        public string adminPword = "secret";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChangeUserPass(string UserName, string PassWord, string RPassWord, string OldPassword)
        {
            string userName = UserName.ToString();
            string newPassword = PassWord.ToString();
            string OldPass = OldPassword.ToString();
            string RPass = RPassWord.ToString();
            TempData["msg"] = "";

            if (newPassword == RPass)
            {
                // Creating an LdapConnection instance 
                Novell.Directory.Ldap.LdapConnection ldapConn = new Novell.Directory.Ldap.LdapConnection();

                string dn = "uid=" + userName + ",ou=users,dc=example,dc=com";

                // Check if User Exists in LDAP
                if (CheckUser(userName, OldPass) == true)
                {
                    try
                    {
                        //Connect function will create a socket connection to the server
                        ldapConn.Connect(ldapHost, ldapPort);

                        //Bind function will Bind the user object Credentials to the Server
                        ldapConn.Bind(adminUname, adminPword);

                        ArrayList modList = new ArrayList();

                        //Replace the existing email  with the new email value
                        LdapAttribute attributes = new LdapAttribute("userPassword", newPassword);
                        modList.Add(new LdapModification(LdapModification.REPLACE, attributes));

                        LdapModification[] mods = new LdapModification[modList.Count];
                        Type mtype = Type.GetType("Novell.Directory.LdapModification");
                        mods = (LdapModification[])modList.ToArray(typeof(LdapModification));

                        //Modify the entry in the directory
                        ldapConn.Modify(dn, mods);
                    }

                    catch (Novell.Directory.Ldap.LdapException e)
                    {
                        string error = "Error: " + e;
                        TempData["msg"] = "<script>alert('" + error + "');</script>";
                        Thread.Sleep(2000);
                        return View("Index");
                    }


                    finally
                    {
                        // Disconnect from LDAP
                        ldapConn.Disconnect();
                    }

                    TempData["msg"] = "<script>alert('Password Changed Successfully!');</script>";
                    Thread.Sleep(2000);
                    return View("Index");
                }

                else
                {
                    TempData["msg"] = "<script>alert('Could not authenticate user');</script>";
                    Thread.Sleep(2000);
                    return View("Index");
                }
            }

            else
            {
                TempData["msg"] = "<script>alert('New passwords do not match!');</script>";
                Thread.Sleep(2000);
                return View("Index");
            }

            
        }

        public bool CheckUser(string UserName, string OldPassword)
        {
            bool result = true;
            string User = UserName;
            string Pass = OldPassword;

            // Creating an LdapConnection instance 
            Novell.Directory.Ldap.LdapConnection ldapConn = new Novell.Directory.Ldap.LdapConnection();

            string dn = "uid = " + UserName + ",ou=users,dc=example,dc=com";

            try
            {
                //Connect function will create a socket connection to the server
                ldapConn.Connect(ldapHost, ldapPort);

                //Bind function will Bind the user object Credentials to the Server
                ldapConn.Bind(dn, OldPassword);
            }

            catch (Novell.Directory.Ldap.LdapException e)
            {
                TempData["msg"] = "<script>alert('Could not authenticate user!');</script>";
                result = false;
                return result;
            }

            finally
            {
                // Disconnect from LDAP
                ldapConn.Disconnect();
            }

            return result;
        }
    }
}