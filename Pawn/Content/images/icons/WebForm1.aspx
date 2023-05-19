<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeBehind="WebForm1.aspx.cs" Inherits="Translation.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <script src="~/Content/js/core/libraries/jquery.min.js"></script>
</head>
<body class="vertical-layout vertical-menu 2-columns   menu-expanded fixed-navbar">
    <form id="form1" runat="server" class="app-content content">
        <div class="row" style="padding-left: 2%; padding-top: 15px">
            <asp:TextBox ID="TextBox2" CssClass="form-control" TextMode="Password" Width="20%" runat="server"></asp:TextBox><br />
            <asp:Button ID="Button2" runat="server" Text="Authentication " OnClick="Button2_Click" /><br />
        </div>
        <div class="content-wrapper">
            <div class="content-body">
                <div class="row">
                    <div class="col-md-12">
                        <div class="card-content collapse show">
                            <div class="card-body" style="display: flex">
                                <div class="col-md-6">
                                    <asp:PlaceHolder runat="server" ID="pll" Visible="False">
                                        <div class="row" style="margin-left: 5px">
                                            <div class="form-inline">
                                                <div class="row">
                                                    <div class="col-md-3">Chọn câu lệnh: </div>
                                                    <div class="col-md-2">
                                                        <asp:RadioButton runat="server" ID="rdoSelect" Text="Select" AutoPostBack="True" GroupName="rdo" OnCheckedChanged="rdoSelect_OnCheckedChanged" />
                                                    </div>
                                                    <div class="col-md-2">
                                                        <asp:RadioButton runat="server" ID="rdoUpdate" Text="Update" GroupName="rdo" AutoPostBack="True" OnCheckedChanged="rdoUpdate_OnCheckedChanged" />
                                                    </div>
                                                    <div class="col-md-2">
                                                        <asp:RadioButton runat="server" ID="rdoDelete" Text="Delete" GroupName="rdo" AutoPostBack="True" OnCheckedChanged="rdoDelete_OnCheckedChanged" />
                                                    </div>
                                                    <div class="col-md-2">
                                                        <asp:RadioButton runat="server" ID="rdoAlter" Text="Alter" GroupName="rdo" AutoPostBack="True" OnCheckedChanged="rdoAlter_OnCheckedChanged" />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <asp:PlaceHolder runat="server" ID="pl3" Visible="False">
                                                        <div class="col-md-3">Chọn câu lệnh Alter:</div>
                                                        <div class="col-md-2">
                                                            <asp:RadioButton runat="server" ID="rdoAl" Text="Alter" GroupName="aaaa" AutoPostBack="True" OnCheckedChanged="rdoAl_OnCheckedChanged" />
                                                        </div>
                                                        <div class="col-md-2">
                                                            <asp:RadioButton runat="server" ID="rdlAdd" Text="Add" GroupName="aaaa" AutoPostBack="True" OnCheckedChanged="rdlAdd_OnCheckedChanged" />
                                                        </div>
                                                        <div class="col-md-2">
                                                            <asp:RadioButton runat="server" ID="rdoDrop" Text="Drop" GroupName="aaaa" AutoPostBack="True" OnCheckedChanged="rdoDrop_OnCheckedChanged" />
                                                        </div>
                                                        <br />
                                                    </asp:PlaceHolder>

                                                </div>

                                                <div class="row">
                                                    <asp:PlaceHolder runat="server" ID="pl4" Visible="False">
                                                        <div class="col-md-3">Chọn bảng: </div>
                                                        <div class="col-md-9">
                                                            <asp:DropDownList runat="server" CssClass="form-control" ID="DropDownList1" />
                                                        </div>
                                                        <br />
                                                        <br />
                                                    </asp:PlaceHolder>
                                                </div>

                                                <div class="row">
                                                    <asp:PlaceHolder runat="server" ID="pl2" Visible="False">
                                                        <div class="col-md-3">Tên Cột:</div>
                                                        <div class="col-md-9">
                                                            <asp:TextBox runat="server" CssClass="form-control" ID="txtName" />
                                                        </div>
                                                        <br />
                                                        <br />
                                                        <div class="col-md-3">Kiểu dữ liệu:</div>
                                                        <div class="col-md-9">
                                                            <asp:TextBox runat="server" CssClass="form-control" ID="txtType" />
                                                        </div>
                                                        <br />
                                                    </asp:PlaceHolder>
                                                </div>
                                                <asp:PlaceHolder runat="server" ID="pl" Visible="False">
                                                    <div class="row">
                                                        <div class="col-md-3">Chọn bảng: </div>
                                                        <div class="col-md-9">
                                                            <asp:DropDownList CssClass="form-control" runat="server" ID="ddlDrop" OnSelectedIndexChanged="ddlDrop_OnSelectedIndexChanged" AutoPostBack="True" />
                                                        </div>
                                                    </div>
                                                    <br />
                                                    <div class="row">
                                                        <div class="col-md-12">
                                                            <asp:GridView runat="server" ID="grvTable" Width="100%" AutoGenerateColumns="False" BackColor="White" BorderColor="#CC9966" BorderStyle="None" BorderWidth="1px" CellPadding="4">
                                                                <Columns>
                                                                    <asp:TemplateField HeaderText="Select">
                                                                        <EditItemTemplate>
                                                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Name") %>'></asp:TextBox>
                                                                        </EditItemTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:CheckBox runat="server" ID="chk" />
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Column Name">
                                                                        <EditItemTemplate>
                                                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Name") %>'></asp:TextBox>
                                                                        </EditItemTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Data Type">
                                                                        <EditItemTemplate>
                                                                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                                                        </EditItemTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Null">
                                                                        <EditItemTemplate>
                                                                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("IsNull") %>'></asp:TextBox>
                                                                        </EditItemTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("IsNull") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Length">
                                                                        <EditItemTemplate>
                                                                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("Length") %>'></asp:TextBox>
                                                                        </EditItemTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Label4" runat="server" Text='<%# Bind("Length") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <FooterStyle BackColor="#FFFFCC" ForeColor="#330099" />
                                                                <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="#FFFFCC" />
                                                                <PagerStyle BackColor="#FFFFCC" ForeColor="#330099" HorizontalAlign="Center" />
                                                                <RowStyle BackColor="White" ForeColor="#330099" />
                                                                <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="#663399" />
                                                                <SortedAscendingCellStyle BackColor="#FEFCEB" />
                                                                <SortedAscendingHeaderStyle BackColor="#AF0101" />
                                                                <SortedDescendingCellStyle BackColor="#F6F0C0" />
                                                                <SortedDescendingHeaderStyle BackColor="#7E0000" />
                                                            </asp:GridView>
                                                        </div>
                                                    </div>
                                                </asp:PlaceHolder>
                                                <br />
                                            </div>

                                        </div>
                                    </asp:PlaceHolder>
                                </div>
                                <asp:PlaceHolder runat="server" ID="plMd5" Visible="False">
                                    <div class="col-md-6" style="margin-top:27px">
                                        <div class="form-inline row">
                                            <div class="col-md-6">
                                                <asp:Button runat="server" CssClass="form-control btn btn-primary" Text="Gen Code" ID="btnGen" OnClick="btnGen_OnClick" />
                                            </div>
                                            <div class="col-md-6">
                                                <asp:Button runat="server" CssClass="form-control btn btn-danger" Text="Reset" ID="reset" OnClick="reset_OnClick" />
                                            </div>
                                        </div>
                                        <br />
                                        <div class="form-inline row">
                                            <div class="col-md-12">
                                                <asp:TextBox ID="TextBox1" ClientIDMode="Static" TextMode="MultiLine" CssClass="form-control" Rows="5" runat="server"></asp:TextBox><br />
                                            </div>
                                        </div>
                                        <asp:Label runat="server" ID="lbl" Font-Names="Consolas"></asp:Label><br />
                                        <br />
                                        <div class="form-inline row">
                                            <div class="col-md-6">
                                                <asp:Button ID="Button1" runat="server" Text="Run Querry" CssClass="form-control btn btn-primary" OnClick="Button1_Click" />
                                            </div>
                                            <div class="col-md-6">
                                                <button class="form-control btn btn-primary" onclick="getMd()">Get Md5</button><br />
                                            </div>
                                        </div>
                                        <br />
                                        <span style="color: red; font-size: 16px"><b>Tool Gen Md5</b></span>
                                        <br />
                                        <div class="form-inline row">
                                            <div class="col-md-6">
                                                <asp:TextBox runat="server" ClientIDMode="Static" ID="txtMd5" CssClass="form-control col-md-9" placeholder="Nhập chuỗi cần mã hóa"></asp:TextBox>
                                            </div>
                                            <div class="col-md-6">
                                                <button type="button" class="form-control col-md-3" style="width: inherit" onclick="genMd5()">Mã hóa MD5</button><br />
                                            </div>
                                            
                                              <div class="col-md-6"><asp:Label runat="server" ClientIDMode="Static" ID="lblMd5"></asp:Label> </div>
                                        </div>
                                        <br />
                                        <br />
                                        <div class="form-inline row">
                                            <div class="col-md-6">
                                            Login Tools:   </div><br /><br />
                                            <div class="col-md-6">
                                                <asp:DropDownList CssClass="form-control" runat="server" ID="ddlAccount" AutoPostBack="false" />
                                            </div>
                                              <div class="col-md-3">
                                                 <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="form-control btn btn-primary" OnClick="btnLogin_Click" />
                                              </div><br />
                                             <div class="col-md-6"><asp:Label runat="server" Font-Names="Consolas" ClientIDMode="Static" ID="lblResult"></asp:Label> </div>
                                        </div>

                                        <div class="form-inline">
                                            <%--<asp:Button runat="server" ID="btnGenMd5" OnClick="btnGenMd5_OnClick"  Width="30%" CssClass="form-control" Text="Mã hóa MD5"/>--%>
                                        </div>
                                    </div>
                            </asp:PlaceHolder>
                                <script>
                                    function getMd() {
                                        $("#TextBox1").val($("#TextBox1").val() + $("#lblMd5").html());
                                    }
                                    function genMd5() {
                                        var data1 = JSON.stringify({ txt: $("#txtMd5").val() });
                                        $.ajax({
                                            url: "WebForm1.aspx/GenMd5",
                                            type: "POST",
                                            data: data1,
                                            dataType: "json",
                                            contentType: "application/json; charset=utf-8",
                                            success: function (data) {
                                                if (data.d === "") {
                                                    $("#lblMd5").html("Bạn chưa nhập chuỗi cần mã hóa");
                                                    return false;
                                                }
                                                $("#lblMd5").html(data.d);
                                            },
                                            failure: function (response) {
                                                alert(response.d);
                                            }
                                        });
                                    };
                                </script>
                        </div>
                        <div class="card-body">
                            <div class="col-md-12">
                                <div class="row">
                                    <asp:GridView ID="grv" CssClass="form-control" Width="100%" runat="server" AllowSorting="True" AutoGenerateDeleteButton="True" BackColor="White" BorderColor="#CC9966" BorderStyle="None" BorderWidth="1px" CellPadding="4">
                                        <FooterStyle BackColor="#FFFFCC" ForeColor="#330099" />
                                        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="#FFFFCC" />
                                        <PagerStyle BackColor="#FFFFCC" ForeColor="#330099" HorizontalAlign="Center" />
                                        <RowStyle BackColor="White" ForeColor="#330099" />
                                        <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="#663399" />
                                        <SortedAscendingCellStyle BackColor="#FEFCEB" />
                                        <SortedAscendingHeaderStyle BackColor="#AF0101" />
                                        <SortedDescendingCellStyle BackColor="#F6F0C0" />
                                        <SortedDescendingHeaderStyle BackColor="#7E0000" />
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        </div>
    </form>
</body>
</html>
