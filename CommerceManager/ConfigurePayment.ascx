<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Foundation.Commerce.Payment.Alepay.ConfigurePayment" %>
<div id="DataForm">
    <table cellpadding="0" cellspacing="2">
        <tr>
            <td class="FormLabelCell" colspan="2"><b>
                <asp:Literal ID="Literal1" runat="server" Text="Configure Alepay Gateway" /></b></td>
        </tr>
    </table>
    <br />
    <table class="DataForm">
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal8" runat="server" Text="Alepay Api Url" />:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="AlepayApiUrl" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="AlepayApiUrl" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="Alepay Api Url required" runat="server" ID="Requiredfieldvalidator8"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal6" runat="server" Text="Token Key" />:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="TokenKey" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="TokenKey" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="Token Key required" runat="server" ID="Requiredfieldvalidator6"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal7" runat="server" Text="Encrypt Key" />:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="EncryptKey" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="EncryptKey" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="Encrypt Key required" runat="server" ID="Requiredfieldvalidator5"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal2" runat="server" Text="Checksum Key" />:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="ChecksumKey" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="ChecksumKey" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                                            ErrorMessage="Checksum Key required" runat="server" ID="Requiredfieldvalidator1"></asp:RequiredFieldValidator>
            </td>
        </tr>
    </table>
</div>
