﻿
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.IO
Imports System.Media
Imports System.Threading
Imports System.Threading.Tasks

Imports SmartBot.Plugins
Imports SmartBot.Plugins.API

Imports SmartBotKit.Interop

#End Region

#Region " ServerDownHandlerPlugin "

Namespace ServerDownHandler

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' This plugin will handles the bot behavior when the server is down.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <seealso cref="Plugin"/>
    ''' ----------------------------------------------------------------------------------------------------
    Public NotInheritable Class ServerDownHandlerPlugin : Inherits Plugin

#Region " Properties "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the plugin's data container.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <value>
        ''' The plugin's data container.
        ''' </value>
        ''' ----------------------------------------------------------------------------------------------------
        Public Shadows ReadOnly Property DataContainer As ServerDownHandlerPluginData
            Get
                Return DirectCast(MyBase.DataContainer, ServerDownHandlerPluginData)
            End Get
        End Property

#End Region

#Region " Private Fields "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Keeps track of the last <see cref="ServerDownHandlerPluginData.Enabled"/> value.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Private lastEnabled As Boolean

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Keeps track of the creation datetime of this plugin.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Private lastDateActive As Date

#End Region

#Region " Constructors "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ServerDownHandlerPlugin"/> class.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Sub New()
            Me.IsDll = True
            SmartBotKit.ReservedUse.UpdateUtil.RunUpdaterExecutable()
            Me.lastDateActive = Date.Now()
        End Sub

#End Region

#Region " Public Methods "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when this <see cref="ServerDownHandlerPlugin"/> is created by the SmartBot plugin manager.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnPluginCreated()
            Me.lastEnabled = Me.DataContainer.Enabled
            If (Me.lastEnabled) Then
                Bot.Log("[ServerDownHandler] Plugin initialized.")
            End If
            MyBase.OnPluginCreated()
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when the properties of <see cref="ServerDownHandlerPluginData"/> are updated.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnDataContainerUpdated()
            Dim enabled As Boolean = Me.DataContainer.Enabled
            If (enabled <> Me.lastEnabled) Then
                If (enabled) Then
                    Bot.Log("[ServerDownHandler] Plugin enabled.")
                Else
                    Bot.Log("[ServerDownHandler] Plugin disabled.")
                End If
                Me.lastEnabled = enabled
            End If
            MyBase.OnDataContainerUpdated()
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when the bot is started.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnStarted()
            Me.lastDateActive = Date.Now
            MyBase.OnStarted()
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when the bot is stopped.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnStopped()
            Me.lastDateActive = Date.Now
            MyBase.OnStopped()
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when the bot is about to handle mulligan (to decide which card to replace) before a game begins.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <param name="choices">
        ''' The mulligan choices.
        ''' </param>
        ''' 
        ''' <param name="opponentClass">
        ''' The opponent class.
        ''' </param>
        ''' 
        ''' <param name="ownClass">
        ''' Our hero class.
        ''' </param>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnHandleMulligan(ByVal choices As List(Of Card.Cards), ByVal opponentClass As Card.CClass, ByVal ownClass As Card.CClass)
            Me.lastDateActive = Date.Now
            MyBase.OnHandleMulligan(choices, opponentClass, ownClass)
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Called when the bot timer is ticked, every 300 milliseconds.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub OnTick()
            If (Me.DataContainer.Enabled) AndAlso (Bot.IsBotRunning()) Then

                Dim lastServerDownRecord As TimeSpan = SmartBotUtil.LastServerDownRecord
                If (lastServerDownRecord = Nothing) Then
                    Exit Sub
                End If

                If (lastServerDownRecord < Me.lastDateActive.TimeOfDay) Then
                    Exit Sub
                End If

                Dim diffTime As TimeSpan = lastServerDownRecord.Subtract(Date.Now.TimeOfDay)
                If (diffTime.TotalSeconds < 60) Then ' If lastServerDownRecord occured more than 1 minute ago then...

                    Dim isBotStopped As Boolean
                    Select Case Bot.CurrentMode

                        Case Bot.Mode.ArenaAuto
                            If (Me.DataContainer.StopTheBotIfArena) Then
                                Bot.StopBot()
                                isBotStopped = True
                            End If

                        Case Bot.Mode.RankedStandard, Bot.Mode.RankedWild
                            If (Me.DataContainer.StopTheBotIfRanked) Then
                                Bot.StopBot()
                                isBotStopped = True
                            End If

                        Case Bot.Mode.UnrankedStandard, Bot.Mode.UnrankedWild
                            If (Me.DataContainer.StopTheBotIfUnranked) Then
                                Bot.StopBot()
                                isBotStopped = True
                            End If

                        Case Else
                            ' Do Nothing.

                    End Select

                    If (isBotStopped) Then
                        Bot.Log("[ServerDownHandler] Server down detected. Bot has been stopped.")

                        If (Me.DataContainer.ResumeEnabled()) Then
                            Dim minutes As Integer = Me.DataContainer.ResumeInterval
                            Me.ScheduleResume(minutes)
                            Bot.Log(String.Format("[ServerDownHandler] Bot resumption scheduled to {0} minutes.", minutes))
                        End If
                    End If

                    If (Me.DataContainer.PlaySoundFile) Then
                        Try
                            Using player As New SoundPlayer(Path.Combine(SmartBotUtil.PluginsDir.FullName, "ServerDownHandler.wav"))
                                player.Play()
                            End Using

                        Catch ex As Exception
                            Bot.Log("[ServerDownHandler] Failed to play the sound file. Error message: " & ex.Message)

                        End Try
                    End If

                End If

            End If

            MyBase.OnTick()
        End Sub

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Releases all the resources used by this <see cref="ServerDownHandlerPlugin"/> instance.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Overrides Sub Dispose()
            MyBase.Dispose()
        End Sub

#End Region

#Region " Private Methods "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Schedule a bot resume to try a reconnection to the server.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Private Sub ScheduleResume(ByVal minutes As Integer)

            Dim resumeMethod As New Action(
                Sub()
                    If Not (Me.DataContainer.ResumeEnabled) Then
                        Exit Sub
                    End If

                    Dim lastDateActive As Date = Me.lastDateActive
                    Thread.Sleep(TimeSpan.FromMinutes(minutes))

                    If (Me.DataContainer.Enabled) AndAlso
                       (Me.DataContainer.ResumeEnabled) AndAlso
                       (Me.lastDateActive = lastDateActive) Then

                        If Not (Bot.IsBotRunning()) Then
                            Bot.StartBot()
                            Bot.Log("[ServerDownHandler] Bot resumed.")
                        End If
                    End If
                End Sub)

            Dim resumeTask As New Task(resumeMethod)
            resumeTask.Start()

        End Sub

#End Region

    End Class

End Namespace

#End Region