using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;

namespace WebService2
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        //[WebMethod]
        //[WebMethod(Description = "Move on the highway!")]
        //public string HelloWorld(string sMg)
        //{
        //    return "Hello World"+sMg;
        //}
        [WebMethod]
        private void ConnectToDb() //not used
        {

                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
                scsb.DataSource = "x.x.x.x";
                scsb.InitialCatalog = "info";
                scsb.IntegratedSecurity = false; // set to true is using Windows Authentication
                scsb.UserID = "appresource";
                scsb.Password = "xxxx";
                SqlConnection conn = new SqlConnection(scsb.ConnectionString);
                try
                {
                    conn.Open();
                    //MessageBox.Show("Connection Successful");
                }
                catch (SqlException loginError)
                {
                     //MessageBox.Show("Failed to connect");
                }
                finally
                {
                    conn.Dispose();
                }

        }

        [WebMethod]
        private string ConnectToRecord(string msisdn, string serviceshortcode, string keyword, string methodType)
        {
            string connetionString = null;
            SqlConnection cnn;
            SqlCommand cmd;
            string sql = null;
            SqlDataReader reader;
            String returnvalue = "";
            string returnquery = "";
            string isActivated = "";
            string ActivatonMode = "";
            //String[] returnxml = null;
            //int no = 0;
            if (serviceshortcode == "") serviceshortcode = null;
            if (keyword == "") keyword = null;

            //----------method type
            switch (methodType)
                {
                   case "s":
                      returnquery = " subscription_enabled,msg_keyword,creation_time,comment";
                      break;
                   case "a":
                      returnquery = " subscription_enabled,msg_keyword,creation_time,comment";
                      //isActivated = " and subscription_enabled='1'";
                      break;
                   case "d":
                      returnquery = " subscription_enabled,msg_keyword,creation_time,comment";
                      break;
                   default:
                      returnquery = " telco_inShortCode,telco_inMobileNo,msg_keyword,comment";
                      break;
                }
            //------------------end
            //connetionString = "Data Source=SHARIF-PC;Initial Catalog=airtel;User ID=sa;Password=btraccl12345678";
            connetionString = "Data Source=XXXXXXXXX;Initial Catalog=info;User ID=appresource;Password=XXXXXXX"; //x.x.x.x
            sql = "SELECT "+returnquery+
            " FROM [info].[dbo].[tbl_subscription_general_user]";

            if (msisdn != null && serviceshortcode != null && keyword != null)
            {
                sql = sql + " where telco_inMobileNo=" + msisdn + " and msg_keyword='" + keyword + "' and telco_inShortCode='" + serviceshortcode + "'";
            }
            else if (msisdn != null && serviceshortcode != null && keyword == null)
            {
                sql = sql + " where telco_inMobileNo=" + msisdn + " and telco_inShortCode='" + serviceshortcode + "'";
            }
            else if (msisdn != null && serviceshortcode == null && keyword == null)
            {
                sql = sql + " where telco_inMobileNo=" + msisdn;
            }
            //if (methodType == "a")
            //    sql = sql + isActivated;

            cnn = new SqlConnection(connetionString);
            try
            {
                cnn.Open();
                cmd = new SqlCommand(sql, cnn);
                reader = cmd.ExecuteReader();
                if (reader.HasRows && methodType == "s") // search option return 1
                {
                    while (reader.Read())
                    {
                        //Console.WriteLine(reader.GetValue(0) + " - " + reader.GetValue(1) + " - " + reader.GetValue(2));
                        //isActivated = reader.GetValue(0).ToString();
                        isActivated = (reader.GetValue(0).ToString() == "True") ? "active" : "inactive";
                        ActivatonMode = (reader.GetValue(3).ToString() == null) ? "sms" : "from_panel";
                        if (returnvalue != "")
                            returnvalue = returnvalue + "," + "ActiveService=" + isActivated + "&ActivationTime=" + reader.GetValue(2) + "&ActivationMode=" + ActivatonMode;
                        else
                            returnvalue = "ActiveService=" + isActivated + "&ActivationTime=" + reader.GetValue(2) + "&ActivationMode=" + ActivatonMode;
                    }
                }
                else if (reader.HasRows == false && methodType == "s") // search option return 2
                {
                    returnvalue = "ActiveService=inactive&ActivationTime=" + DateTime.Now + "&ActivationMode=no";
                }
                else if (reader.HasRows && methodType == "a")
                {
                    while (reader.Read())
                    {
                        //returnvalue = "update";
                        returnvalue = (reader.GetValue(0).ToString() == "True") ? "Status=fail&DateTime=" + reader.GetValue(2) + "&Reasons=activated" : "update";
                        //returnvalue = "Status=success&DateTime=" + DateTime.Now + "&Reasons=activated_from_API";
                        //returnvalue = "status=fail&VASName=" + servicename;
                    }
                }
                else if (reader.HasRows == false && methodType == "a")
                {
                    returnvalue = "insert";
                    //returnvalue = "status=fail&VASName=" + servicename;
                }
                else if (reader.HasRows && methodType == "d")
                {
                    while (reader.Read())
                    {
                        returnvalue = (reader.GetValue(0).ToString() == "False") ? "Status=fail&DateTime=" + reader.GetValue(2) + "&Reasons=deactivated" : "update";
                        //returnvalue = "Status=success&DateTime=" + DateTime.Now + "&Reasons=activated_from_API";
                        //returnvalue = "status=fail&VASName=" + servicename;
                    }
                }
                else if (reader.HasRows == false && methodType == "d")
                {
                    //returnvalue = "status=fail&VASName=" + keyword;
                    returnvalue = "Status=fail&DateTime=" + DateTime.Now + "&Reasons=not_subscribed";
                    //returnvalue = "status=fail&VASName=" + servicename;
                }

                reader.Close();
                cmd.Dispose();
                cnn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection ! ");
            }
            return returnvalue;
        }


        [WebMethod]
        private string InsertToRecord(string msisdn, string serviceshortcode, string keyword, string methodType)
        {
            string connetionString = null;
            SqlConnection con;
            SqlCommand cmd;
            string qry = null;
            String returnvalue = "";
            //connetionString = "Data Source=SHARIF-PC;Initial Catalog=airtel;User ID=sa;Password=btraccl12345678";
            connetionString = "Data Source=x.x.x.x;Initial Catalog=info;User ID=appresource;Password=xxxx";
            // update_time,+ DateTime.Now.ToString("YY-MM-DD HH:mm:ss")
            qry = "insert into tbl_subscription_general_user(telco_inShortCode,telco_inMobileNo,msg_keyword,catagory,subscription_enabled,enabled,comment) values('" + serviceshortcode + "','" + msisdn + "','" + keyword + "','OPEN','" + 1 + "','" + 1 + "','Subscribed from API')";
            con = new SqlConnection(connetionString);
            cmd = new SqlCommand(qry, con);
            if (con.State == ConnectionState.Open)
                con.Close();
            else
                con.Open();
            cmd.ExecuteNonQuery();
            //if (roweffect > 0)
            //{
            //    returnvalue = "status=success&vasname=" + servicename;
            //}
            //else
            //{
            //    returnvalue = "status=fail&vasname=" + servicename;
            //}
            //returnvalue = "status=success&DateTime=" + servicename;
            returnvalue = "Status=success&DateTime=" + DateTime.Now + "&Reasons=activated_from_API";
            con.Close();
            Console.WriteLine("Can not open connection ! ");
            return returnvalue;
        }

        [WebMethod]
        private string UpdateToRecord(string msisdn, string serviceshortcode, string keyword, string methodType)
        {
            string connetionString = null;
            SqlConnection con;
            SqlCommand cmd;
            string qry = null;
            String returnvalue = "";
            int updateValue = 0;
            string reasons = "deactivated from API";
            if (methodType == "a")
            {
                updateValue = 1;
                reasons = "activated from API";
            }

            //connetionString = "Data Source=SHARIF-PC;Initial Catalog=airtel;User ID=sa;Password=btraccl12345678";
            connetionString = "Data Source=x.x.x.x;Initial Catalog=info;User ID=appresource;Password=xxxx";
            // update_time,+ DateTime.Now.ToString("YY-MM-DD HH:mm:ss")
            qry = "update tbl_subscription_general_user set subscription_enabled='" + updateValue + "',comment='" + reasons + "', update_time='" + DateTime.Now + "',catagory='OPEN' where telco_inMobileNo='" + msisdn + "' and telco_inShortCode='" + serviceshortcode + "' and msg_keyword='" + keyword + "'";
            con = new SqlConnection(connetionString);
            cmd = new SqlCommand(qry, con);
            if (con.State == ConnectionState.Open)
                con.Close();
            else
                con.Open();
            cmd.ExecuteNonQuery();
            //if (roweffect > 0)
            //{
            //    returnvalue = "status=success&vasname=" + servicename;
            //}
            //else
            //{
            //    returnvalue = "status=fail&vasname=" + servicename;
            //}
            returnvalue = "Status=success&DateTime=" + DateTime.Now + "&Reasons='" + reasons+"'";
            con.Close();
            Console.WriteLine("Can not open connection ! ");
            return returnvalue;
        }

        [WebMethod]
        private string checkMSISDN(string msisdn)
        {
            string returnvalue = msisdn.Substring(0, 5) == "88018" ? "T" : "F";
            //string[] showvalue =  returnvalue.Split(new string[] { "," }, StringSplitOptions.None);
            return returnvalue; //showvalue[]; //"mobile:"+msisdn+" and vas:"+vasname;
        }

        [WebMethod]
        public string searchService(string msisdn, string serviceshortcode)
        {

            string mobilecheck = checkMSISDN(msisdn);
            string returnvalue = "";
            if (mobilecheck == "F")
            {
                returnvalue = "ActiveService=inactive&ActivationTime=" + DateTime.Now + "&ActivationMode=no";
            }
            else
            {
                returnvalue = ConnectToRecord(msisdn, serviceshortcode, null, "s"); // short code and no keyword like cric
            }
            return returnvalue; //showvalue[]; //"mobile:"+msisdn+" and vas:"+vasname;
         }

       [WebMethod]
        public string activate(string msisdn, string serviceshortcode, string keyword)
        {
           string returnvalue="";
           string mobilecheck = checkMSISDN(msisdn);
           if (mobilecheck == "F")
           {
               //returnvalue = "status=fail&VASName=" + serviceshortcode;
               returnvalue = "Status=fail&DateTime=" + DateTime.Now + "&Reasons=not_operator";
           }
           else
           {
               string whatvalue = ConnectToRecord(msisdn, serviceshortcode, keyword, "a");
               if (whatvalue == "update")
                   returnvalue = UpdateToRecord(msisdn, serviceshortcode, keyword, "a");
               else if (whatvalue == "insert")
                   returnvalue = InsertToRecord(msisdn, serviceshortcode, keyword, "a");
               else returnvalue = whatvalue;
           }
           return returnvalue;
        }

       [WebMethod]
       public string deactivate(string msisdn, string serviceshortcode, string keyword)
        {
            string returnvalue = "";
            string mobilecheck = checkMSISDN(msisdn);
            if (mobilecheck == "F")
            {
                //returnvalue = "status=fail&VASName=" + keyword;
                returnvalue = "Status=fail&DateTime=" + DateTime.Now + "&Reasons=not_operator";
            }
            else
            {
                string whatvalue = ConnectToRecord(msisdn, serviceshortcode, keyword, "d");
                if (whatvalue == "update")
                    returnvalue = UpdateToRecord(msisdn, serviceshortcode, keyword, "d");
                else returnvalue = whatvalue;
            }
           return returnvalue;
        }

        // [WebMethod]
        //public double FahrenheitToCelsius(double Fahrenheit)
        //{
        //    return ((Fahrenheit - 32) * 5) / 9;
        //}

        // [WebMethod]
        //public double CelsiusToFahrenheit(double Celsius)
        //{
        //    return ((Celsius * 9) / 5) + 32;
        //}
    }
}
