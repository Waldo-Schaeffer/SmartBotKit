﻿
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Automation

Imports SmartBot.Plugins

Imports SmartBotKit.Interop.Win32

#End Region

#Region " SmartBotUtil "

Namespace SmartBotKit.Interop

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Provides reusable automation utilities for SmartBot process.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public NotInheritable Class SmartBotUtil

#Region " Properties "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the SmartBot <see cref="Diagnostics.Process"/>.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The SmartBot <see cref="Diagnostics.Process"/>.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared ReadOnly Property Process As Process
            <DebuggerStepThrough>
            Get
                If (SmartBotUtil.processB Is Nothing) OrElse (SmartBotUtil.processB.HasExited) Then
                    SmartBotUtil.processB = Diagnostics.Process.GetCurrentProcess()
                End If
                ' SmartBotUtil.processB.Refresh() ' Refresh window title and main window handle.
                Return SmartBotUtil.processB
            End Get
        End Property
        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' ( Backing Field )
        ''' <para></para>
        ''' Gets the SmartBot <see cref="Diagnostics.Process"/>.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Private Shared processB As Process

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets a value indicating whether the SmartBot process is displaying the 'Loading' splash screen.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' <see langword="True"/> if SmartBot window is displaying the splashscreen; otherwise, <see langword="False"/>.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared ReadOnly Property IsInSplashScreen As Boolean
            Get
                Return (Process.GetCurrentProcess().MainWindowTitle.StartsWith("Loading", StringComparison.OrdinalIgnoreCase))
            End Get
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets identifier of the thread that created the SmartBot main window; the UI thread.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The identifier of the thread that created the SmartBot main window; the UI thread.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared ReadOnly Property MainThreadId As Integer
            <DebuggerStepThrough>
            Get
                Return NativeMethods.GetWindowThreadProcessId(SmartBotUtil.Process.MainWindowHandle, New Integer)
            End Get
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the statistics string shown in the SmartBot window.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The statistics string shown in the SmartBot window.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared ReadOnly Property Statistics As String
            <DebuggerStepThrough>
            Get
                Return SmartBotUtil.GetStatisticsString(SmartBotUtil.Process)
            End Get
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the current wins ratio percentage string.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The wins ratio percentage string.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared ReadOnly Property WinsRatio As String
            <DebuggerStepThrough>
            Get
                Return SmartBotUtil.GetWinsRatio()
            End Get
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets or sets the SmartBot window placement.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The Hearthstone window placement.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared Property WindowPlacement As WindowPlacement
            <DebuggerStepThrough>
            Get
                Return SmartBotUtil.GetWindowPlacement(SmartBotUtil.Process.MainWindowHandle)
            End Get
            Set(value As WindowPlacement)
                Dim wpl As WindowPlacement = SmartBotUtil.GetWindowPlacement(SmartBotUtil.Process.MainWindowHandle)
                If (wpl.NormalPosition <> CType(value.NormalPosition, Rectangle)) OrElse
                   (wpl.WindowState <> value.WindowState) OrElse
                   (wpl.Flags <> value.Flags) OrElse
                   (wpl.MaxPosition <> CType(value.MaxPosition, Point)) OrElse
                   (wpl.MinPosition <> CType(value.MinPosition, Point)) Then

                    SmartBotUtil.SetWindowPlacement(SmartBotUtil.Process.MainWindowHandle, value)
                End If
            End Set
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets or sets the SmartBot window position.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The Hearthstone window position.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared Property WindowPosition As Point
            <DebuggerStepThrough>
            Get
                Return SmartBotUtil.GetWindowPosition(SmartBotUtil.Process.MainWindowHandle)
            End Get
            Set(value As Point)
                If (SmartBotUtil.GetWindowPosition(SmartBotUtil.Process.MainWindowHandle) <> value) Then
                    SmartBotUtil.SetWindowPosition(SmartBotUtil.Process.MainWindowHandle, value)
                End If
            End Set
        End Property

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets or sets the SmartBot window size.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The Hearthstone window size.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shared Property WindowSize As Size
            <DebuggerStepThrough>
            Get
                Return SmartBotUtil.GetWindowSize(SmartBotUtil.Process.MainWindowHandle)
            End Get
            Set(value As Size)
                If (SmartBotUtil.GetWindowSize(SmartBotUtil.Process.MainWindowHandle) <> value) Then
                    SmartBotUtil.SetWindowSize(SmartBotUtil.Process.MainWindowHandle, value)
                End If
            End Set
        End Property

#End Region

#Region " Constructors "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Prevents a default instance of the <see cref="SmartBotUtil"/> class from being created.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets an <see cref="AutomationElement"/> that has the specified automation id. in the SmartBot main window.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The resulting <see cref="AutomationElement"/>.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        <DebuggerStepThrough>
        Public Shared Function GetAutomationElement(ByVal automationId As String) As AutomationElement
            Return SmartBotUtil.GetAutomationElement(SmartBotUtil.Process, automationId)
        End Function

#End Region

#Region " Private Methods "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the statistics string shown in the SmartBot window.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <param name="process">
        ''' The SmartBot <see cref="Diagnostics.Process"/>.
        ''' </param>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The statistics string shown in the SmartBot window.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        <DebuggerStepperBoundary>
        Private Shared Function GetStatisticsString(ByVal process As Process) As String
            Dim element As AutomationElement = SmartBotUtil.GetAutomationElement(process, "Statslabel")

            Return element.Current.Name
        End Function

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the current wins ratio percentage string.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The wins ratio percentage string.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        <DebuggerStepThrough>
        Private Shared Function GetWinsRatio() As String
            Dim wins As Integer = API.Statistics.Wins
            Dim losses As Integer = API.Statistics.Losses
            Dim concedes As Integer = API.Statistics.ConcededTotal
            Dim winsRatio As Double = ((wins / (wins + losses + concedes)) * 100)

            If Double.IsNaN(winsRatio) Then
                winsRatio = 0R
            End If

            Dim str As String
            Select Case winsRatio

                Case = 0
                    str = "0%"

                Case = 100.0R
                    str = "100%"

                Case Else
                    str = String.Format("{0:F2}%", winsRatio)

            End Select

            Return str
        End Function

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets an <see cref="AutomationElement"/> that has the specified automation id. in the SmartBot window.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <param name="process">
        ''' The SmartBot <see cref="Diagnostics.Process"/>.
        ''' </param>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The resulting <see cref="AutomationElement"/>.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        <DebuggerStepThrough>
        Private Shared Function GetAutomationElement(ByVal process As Process, ByVal automationId As String) As AutomationElement
            Dim window As AutomationElement = AutomationElement.FromHandle(process.MainWindowHandle)
            Dim condition As New PropertyCondition(AutomationElement.AutomationIdProperty, automationId)
            Dim element As AutomationElement = window.FindFirst(TreeScope.Subtree, condition)

            Return element
        End Function

        Private Shared Function GetWindowPlacement(ByVal hWnd As IntPtr) As WindowPlacement
            Dim wpl As New WindowPlacement()
            wpl.Length = Marshal.SizeOf(wpl)
            NativeMethods.GetWindowPlacement(hWnd, wpl)
            Return wpl
        End Function

        Private Shared Function SetWindowPlacement(ByVal hWnd As IntPtr, ByVal wpl As WindowPlacement) As Boolean
            Return NativeMethods.SetWindowPlacement(hWnd, wpl)
        End Function

        Private Shared Function GetWindowSize(ByVal hWnd As IntPtr) As Size
            Dim rc As Rectangle
            NativeMethods.GetWindowRect(hWnd, rc)
            Return rc.Size
        End Function

        Private Shared Function SetWindowSize(ByVal hWnd As IntPtr, ByVal sz As Size) As Boolean
            Dim rc As Rectangle
            NativeMethods.GetWindowRect(hWnd, rc)
            Return NativeMethods.SetWindowPos(hWnd, IntPtr.Zero,
                                              rc.Location.X, rc.Location.Y,
                                              sz.Width, sz.Height,
                                              SetWindowPosFlags.IgnoreMove)
        End Function

        Private Shared Function GetWindowPosition(ByVal hWnd As IntPtr) As Point
            Dim rc As Rectangle
            NativeMethods.GetWindowRect(hWnd, rc)
            Return rc.Location
        End Function

        Private Shared Function SetWindowPosition(ByVal hWnd As IntPtr, ByVal pt As Point) As Boolean
            Dim rc As Rectangle
            NativeMethods.GetWindowRect(hWnd, rc)
            Return NativeMethods.SetWindowPos(hWnd, IntPtr.Zero,
                                              pt.X, pt.Y,
                                              rc.Size.Width, rc.Size.Height,
                                              SetWindowPosFlags.IgnoreResize)
        End Function

#End Region

    End Class

End Namespace

#End Region