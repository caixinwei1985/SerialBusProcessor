<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    Friend WithEvents sbp As SerialBusProcessor.SerialBusProcessor
    Private sp As System.IO.Ports.SerialPort
    Private threads(5) As System.Threading.Thread
    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.sp = New System.IO.Ports.SerialPort(Me.components)
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'sp
        '
        Me.sp.BaudRate = 115200
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(131, 128)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

        Me.sbp = New SerialBusProcessor.SerialBusProcessor(5, sp)
        Me.threads(0) = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf Me.test_thread))
        Me.threads(0).Start()

    End Sub
    Private Sub test_thread(ByVal ch As Object)
        MessageBox.Show("test thread start")
    End Sub
    Private Sub command_arrival(ByVal ch As Integer, ByVal buf() As Byte, ByVal length As Integer) Handles sbp.SerialCommandArrival
        MessageBox.Show("command arrival vb.net")
        Button1.Text = "vb.net"
    End Sub
    Friend WithEvents Button1 As Button
End Class
