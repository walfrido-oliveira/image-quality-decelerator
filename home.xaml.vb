Imports System.Threading
Imports System.IO
Imports System.Collections

Class home

    Private trd As Thread
    Private qld As Integer
    Private caminho As String
    Private count As Integer = 0
    Private blnJpeg As Boolean
    Private blnPng As Boolean
    Private oArquivo As System.IO.File
    Private oEscreve As System.IO.StreamWriter
    Private dblTamnhoInit As Long
    Private dblTamanhoFinal As Long
    Private infoReader As System.IO.FileInfo

    Private WithEvents BackgroundWorker1 As New System.ComponentModel.BackgroundWorker()

    ''' <summary>
    ''' DELEGATES PARA USO NA THRED DE CAPTURA E CONVERSÃO DE IMAGENS
    ''' </summary>
    Delegate Sub DSetMaxProgressBar(ByVal value As Integer)
    Delegate Sub DSetVisibleProgressBar(ByVal t As Boolean)
    Delegate Sub DSetLabelTotalArq(ByVal str As String)
    Delegate Sub DSetLabelTotalArqConvert(ByVal str As String)
    Delegate Sub DSetTextTamanhoInit(ByVal str As String)
    Delegate Sub DSetTextTamanhoFinal(ByVal str As String)
    Delegate Sub DSetLabelGanho(ByVal str As String)
    Delegate Sub DSetProgressBar(ByVal value As Integer)

    ''' <summary>
    ''' MOVE JANELA
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Rectangle_MouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles Rectangle1.MouseLeftButtonDown
        DragMove()
    End Sub

    ''' <summary>
    ''' FECHA JANELA
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub btnFecha_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    ''' <summary>
    ''' MINIMIZA JANELA
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub btnMin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MinButton.Click
        Me.WindowState = Windows.WindowState.Minimized
    End Sub

    ''' <summary>
    ''' ABRE BROWSER DE PASTAS DO WINDOWS
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub btnPasta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBusca.Click

        Dim pasta As New System.Windows.Forms.FolderBrowserDialog
        If pasta.ShowDialog = Forms.DialogResult.OK Then
            txtRaiz.Text = pasta.SelectedPath
        End If

    End Sub

    ''' <summary>
    ''' EXECUTA VARREDURA
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub btnIniciar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnIniciar.Click

        qld = CInt(txtQld.Text)
        caminho = txtRaiz.Text

        blnJpeg = chkJpg.IsChecked
        blnPng = chkPng.IsChecked

        lblTotalArq.Content = "0"
        lblTotalConvert.Content = "0"
        lblGanho.Content = "0"
        lblTamanhoInicial.Content = "0"
        lblTamanhoFinal.Content = "0"

        dblTamnhoInit = 0
        dblTamanhoFinal = 0


        If Not BackgroundWorker1.IsBusy Then
            BackgroundWorker1.RunWorkerAsync()
        End If

    End Sub

    ''' <summary>
    ''' INICIA TRABALHO EM SEGUNDO PLANO
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        e.Result = GetFiles(caminho)
    End Sub

    ''' <summary>
    ''' CONCLUÍ TAREFA EM SEGUNDO PLANO
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub BackgroundWorker1_RunWorkerCOmpleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        trd = New Thread(New System.Threading.ParameterizedThreadStart(AddressOf ConvertImg))
        trd.IsBackground = True
        trd.Start(e.Result)
    End Sub

    ''' <summary>
    ''' OBTÉM LISTA DE ARQUIVOS
    ''' </summary>
    ''' <param name="pathFolder">PATH INICIAL</param>
    ''' <returns>LISTA DE ARQUIVOS</returns>
    ''' <remarks></remarks>
    Private Function GetFiles(ByVal pathFolder As String) As List(Of String)

        Dim returnFiles As List(Of String) = New List(Of String)
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(caminho)

        Dim b As New DSetLabelTotalArq(AddressOf SetLabelTotalArq)
        Dim w As New DSetTextTamanhoInit(AddressOf SetTextTamanhoInit)

        oEscreve = File.CreateText("c:\\log.txt")
        oEscreve.WriteLine(".:: LISTAGEM DE ARQUIVOS .:")

        Dim Full() As String = Directory.GetDirectories(pathFolder)
        For Each Dir As String In Full
            Try
                Dim fullFiles() As String = Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories)
                For Each File As String In fullFiles
                    Try
                        If ((blnJpeg And File.ToLower.Contains(".jpg")) Or (blnPng And File.ToLower.Contains(".png"))) _
                        And System.IO.File.Exists(File) Then
                            returnFiles.Add(File)
                            oEscreve.WriteLine(File)
                            infoReader = My.Computer.FileSystem.GetFileInfo(File)
                            dblTamnhoInit += infoReader.Length
                            count += 1
                            Me.lblTotalArq.Dispatcher.Invoke(b, New Object() {String.Format("{0:N0}", count)})
                            Me.lblTamanhoInicial.Dispatcher.Invoke(w, New Object() {String.Format("{0:N0} MB", (dblTamnhoInit / 1048576))})
                        End If
                    Catch ex As Exception
                    End Try
                Next
            Catch ex As Exception
            End Try
        Next
        Return returnFiles

    End Function

    ''' <summary>
    ''' CONVERTE IMAGENS
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ConvertImg(ByVal a As List(Of String))

        Dim cont As Integer = 0

        Dim b As New DSetVisibleProgressBar(AddressOf SetVisibleProgressBar)
        Me.pgb10.Dispatcher.Invoke(b, New Object() {True})

        Dim d As New DSetMaxProgressBar(AddressOf SetMaxProgressBar)
        Me.pgb10.Dispatcher.Invoke(d, New Object() {a.Count})

        Dim f As New DSetProgressBar(AddressOf SetProgressBar)
        Dim g As New DSetLabelTotalArqConvert(AddressOf SetLabelTotalArqConvert)
        Dim w As New DSetTextTamanhoFinal(AddressOf SetTextTamanhoFinal)
        Dim y As New DSetLabelGanho(AddressOf SetLabelGanho)

        Dim objImageCodecInfo() As System.Drawing.Imaging.ImageCodecInfo
        objImageCodecInfo = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders
        Dim objEncParams As New System.Drawing.Imaging.EncoderParameters(1)
        Dim objEncParam As New System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qld) 'definindo qualidade %%
        objEncParams.Param(0) = objEncParam
        Dim img As System.Drawing.Image

        oEscreve.WriteLine(".:: ARQUIVOS CONVERTIDOS .:")

        For x As Integer = 0 To a.Count - 1
            If blnJpeg Then
                If a(x).ToLower.Contains(".jpg") And File.Exists(a(x)) Then
                    img = System.Drawing.Image.FromFile(a(x)) 'imagem de origem

                    'salvando a nova imagem com qualidade de %

                    img.Save(a(x) & "new", objImageCodecInfo(1), objEncParams)
                    img.Dispose()
                    File.Delete(a(x))
                    My.Computer.FileSystem.RenameFile(a(x) + "new", a(x).Substring(a(x).LastIndexOf("\") + 1))
                    cont += 1
                    infoReader = My.Computer.FileSystem.GetFileInfo(a(x))
                    dblTamanhoFinal += infoReader.Length
                    oEscreve.WriteLine(a(x))
                End If
            End If

            If blnPng Then
                If a(x).ToLower.Contains(".png") And File.Exists(a(x)) Then
                    img = System.Drawing.Image.FromFile(a(x)) 'imagem de origem

                    'salvando a nova imagem com qualidade de %

                    img.Save(a(x) & "new", objImageCodecInfo(1), objEncParams)
                    img.Dispose()
                    File.Delete(a(x))
                    My.Computer.FileSystem.RenameFile(a(x) + "new", a(x).Substring(a(x).LastIndexOf("\") + 1))
                    cont += 1
                    infoReader = My.Computer.FileSystem.GetFileInfo(a(x))
                    dblTamanhoFinal += infoReader.Length
                    oEscreve.WriteLine(a(x))
                End If
            End If

            Me.pgb10.Dispatcher.Invoke(f, New Object() {cont})
            Me.lblTotalConvert.Dispatcher.Invoke(g, New Object() {String.Format("{0:N0}", cont)})
            Me.lblTamanhoFinal.Dispatcher.Invoke(w, New Object() {String.Format("{0:N0} MB", (dblTamanhoFinal / 1048576))})
        Next

        Me.pgb10.Dispatcher.Invoke(b, New Object() {False})
        Me.lblGanho.Dispatcher.Invoke(y, New Object() {String.Format("{0:N0} MB", (dblTamnhoInit - dblTamanhoFinal) / 1048576)})

        oEscreve.Close()
        oEscreve.Dispose()

    End Sub

    ''' <summary>
    ''' SETA TEXTO DO LABEL
    ''' </summary>
    ''' <param name="str"></param>
    ''' <remarks></remarks>
    Private Sub SetLabelGanho(ByVal str As String)
        lblGanho.Content = str
    End Sub

    ''' <summary>
    ''' SETA TEXTO
    ''' </summary>
    ''' <param name="str">TEXTO</param>
    ''' <remarks></remarks>
    Private Sub SetTextTamanhoInit(ByVal str As String)
        lblTamanhoInicial.Content = str
    End Sub

    ''' <summary>
    ''' SETA TEXTO
    ''' </summary>
    ''' <param name="str">TEXTO</param>
    ''' <remarks></remarks>
    Private Sub SetTextTamanhoFinal(ByVal str As String)
        lblTamanhoFinal.Content = str
    End Sub

    ''' <summary>
    ''' SETA VISIBILIDADE DO PROGRESSBAR
    ''' </summary>
    ''' <param name="t"></param>
    ''' <remarks></remarks>
    Private Sub SetVisibleProgressBar(ByVal t As Boolean)
        If t Then
            pgb10.Visibility = Windows.Visibility.Visible
        Else
            pgb10.Visibility = Windows.Visibility.Hidden
        End If
    End Sub

    ''' <summary>
    ''' SETA O VALOR MÁXIMO DO PROGRESSBAR
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Private Sub SetMaxProgressBar(ByVal value As Integer)
        pgb10.Maximum = value
    End Sub

    ''' <summary>
    ''' SETA O PROPERTY VALUE DO PROGRESSBAR
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Private Sub SetProgressBar(ByVal value As Integer)
        If value > pgb10.Maximum Then
            pgb10.Visibility = Windows.Visibility.Hidden
        Else
            pgb10.Value = value
        End If

    End Sub

    ''' <summary>
    ''' SETA TEXTO DO LABEL
    ''' </summary>
    ''' <param name="str"></param>
    ''' <remarks></remarks>
    Private Sub SetLabelTotalArq(ByVal str As String)
        lblTotalArq.Content = str
    End Sub

    ''' <summary>
    ''' SETA TEXTO DO LABEL
    ''' </summary>
    ''' <param name="str"></param>
    ''' <remarks></remarks>
    Private Sub SetLabelTotalArqConvert(ByVal str As String)
        lblTotalConvert.Content = str
    End Sub
End Class
