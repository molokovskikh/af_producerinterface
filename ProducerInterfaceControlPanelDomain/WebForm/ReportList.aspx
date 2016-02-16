<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportList.aspx.cs" Inherits="ProducerInterfaceControlPanelDomain.WebForm.ReportList" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
  
    <form id="form1" runat="server">
    <div>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" DataSourceID="EntityDataSource_">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" SortExpression="Id"></asp:BoundField>
                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title"></asp:BoundField>
                <asp:BoundField DataField="Type" HeaderText="Type" SortExpression="Type"></asp:BoundField>
                <asp:BoundField DataField="GeneralReportId" HeaderText="GeneralReportId" SortExpression="GeneralReportId"></asp:BoundField>
                <asp:BoundField DataField="UserId" HeaderText="UserId" SortExpression="UserId"></asp:BoundField>
                <asp:BoundField DataField="Modificator" HeaderText="Modificator" SortExpression="Modificator"></asp:BoundField>
                <asp:BoundField DataField="CreationDate" HeaderText="CreationDate" SortExpression="CreationDate"></asp:BoundField>
                <asp:BoundField DataField="ModificationDate" HeaderText="ModificationDate" SortExpression="ModificationDate"></asp:BoundField>
            </Columns>
        </asp:GridView>
        <asp:EntityDataSource runat="server" ID="EntityDataSource_" DefaultContainerName="producerinterface_Entities" ConnectionString="name=producerinterface_Entities" EnableFlattening="False" EntitySetName="reporttemplate"></asp:EntityDataSource>
    </div>
    </form>
</body>
</html>
