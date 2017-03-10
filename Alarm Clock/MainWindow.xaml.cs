﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Alarm_Clock
{
    public partial class MainWindow : Window
    {
        //these are the objects of the min, hour, and second hand for the analog clock
        private RotateTransform MinHandTr = new RotateTransform();
        private RotateTransform HourHandTr = new RotateTransform();
        private RotateTransform SecHandTr = new RotateTransform();

        public UserAlarm currAlarm;

        private int createAlarmHour = 12;
        private int createAlarmMin = 0;
        private int createAlarmAMPM = 0;

        private System.Media.SoundPlayer player;
        private bool alarmState;

        private static int idSet = 0;

        public int menuTogg = 0;

        public LinkedList<Alarm> alarms = new LinkedList<Alarm>();
        public LinkedList<UserAlarm> uAlarms = new LinkedList<UserAlarm>();

        AlarmRing ring = new AlarmRing();

        public MainWindow()
        {
            //initalizes the clock  
            InitializeComponent();
            alarmState = false;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            ring.AlarmRings += Ring_AlarmRings;

            this.KeyUp += MainWindow_KeyUp;
        }

        public void slideMenuToggle(Canvas slideMenu, int menuTogg)
        {
            moveSlideMenu(slideMenu, menuTogg);
            this.menuTogg = (menuTogg + 1) % 2;

            if (plusButton.Content.ToString() == "-")
            {
                plusButton.Content = "+";
            }
            else
            {
                plusButton.Content = "-";
            }        
        }

        // Animates the slide menu.
        public static void moveSlideMenu(Canvas slideMenu, int menuTogg)
        {
            TranslateTransform trans = new TranslateTransform();
            slideMenu.RenderTransform = trans;
            DoubleAnimation anim = null;

            // Slide menu is hidden, so slide it out
            if (menuTogg == 0)
            {
                anim = new DoubleAnimation(0, -600, TimeSpan.FromSeconds(0.5));
            }

            // Slide menu is visible, so put it back
            else
            {
                anim = new DoubleAnimation(-600, 0, TimeSpan.FromSeconds(0.5));
            }

            trans.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void Ring_AlarmRings(object sender, AlarmEventArgs e)
        {
            if (e.currAl.getAlarm().dismissed == false)
            {
                e.currAl.getAlarm().dismissed = true;
                player = new System.Media.SoundPlayer(e.currAl.getAlarm().getRingerPath());
                player.Load();
                player.Play();
                currAlarm = e.currAl;
                this.alertCanvas1.Visibility = Visibility.Visible;
                this.alertCanvas2.Visibility = Visibility.Visible;
            }
        }

        /* this method is an event driven system for analog and digital clock
         * event is called every "tick" which is one second
         * it also updates the clocks each second
         */
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var myDate = DateTime.Now;

            /* digital clock
             * displays the time, date and am
             */

            digitalTime.Content = myDate.ToString("hh:mm:ss"); //hours:minutes: seconds   
            amORpm.Content = myDate.ToString("tt"); //AM or PM 
            date.Content = myDate.ToString("MMM dd, yyyy"); //month day, year 

            //analog clock
            //calculates angles for the minute, hour and second hand 
            MinHandTr.Angle = (myDate.Minute * 6);
            HourHandTr.Angle = (myDate.Hour * 30) + (myDate.Minute * 0.5);
            SecHandTr.Angle = (myDate.Second * 6);

            //moves the minute, second and hour hand  
            MinuteHand.RenderTransform = MinHandTr;
            HourHand.RenderTransform = HourHandTr;
            SecondHand.RenderTransform = SecHandTr;

            alarmCheck(ring);
        }

        private void plusButton_Click(object sender, RoutedEventArgs e)
        {
            editAlarm_save.Visibility = Visibility.Hidden;
            setAlarm_save.Visibility = Visibility.Visible;
            if (menuTogg == 1)
            {
                //reset the create alarm to inital values
                createAlarmAMPM = 0;
                createAlarmHour = 12;
                createAlarmMin = 0;
                //update the visuals for the reset values
                setAlarm_minutes.Content = "0" + "0";
                setAlarm_hours.Content = 12;
                setAlarm_amORpm.Content = "AM";
            }

            slideMenuToggle(slideMenu, menuTogg);
        }
       
        /* This method closes the program down if the escape key is hit
         */
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        //**When creating a new alarm** - decriment of the mins button  
        private void setAlarm_decMinutes_Click(object sender, RoutedEventArgs e)
        {
            //if there are 0 mins in the label clicking up will change it to 59 mins
            if (createAlarmMin == 0)
            {
                createAlarmMin = 59;
                setAlarm_minutes.Content = createAlarmMin.ToString();
            }
            //if the minutes are inbetween or equal to 0 and 9 then add an extra "0" to format it 
            else if (createAlarmMin >= 0 && createAlarmMin < 11)
            {
                createAlarmMin -= 1;
                setAlarm_minutes.Content = "0" + createAlarmMin.ToString();
            }
            //otherwise just update the current minutes and update the label 
            else
            {
                createAlarmMin -= 1;
                setAlarm_minutes.Content = createAlarmMin.ToString();
            }
        }


        //**When creating a new alarm** - increment of the the mins button
        private void setAlarm_incMinutes_Click(object sender, RoutedEventArgs e)
        {
            //if there are 59 mins in the label clicking up will change it to 00 mins
            if (createAlarmMin == 59)
            {
                createAlarmMin = 0;
                setAlarm_minutes.Content = "0" + createAlarmMin.ToString();
            }
            //if the minutes are inbetween or equal to 0 and 9 then add an extra "0" to format it 
            else if (createAlarmMin >= 0 && createAlarmMin < 9)
            {
                createAlarmMin += 1;
                setAlarm_minutes.Content = "0" + createAlarmMin.ToString();
            }
            //otherwise just update the current minutes and update the label 
            else
            {
                createAlarmMin += 1;
                setAlarm_minutes.Content = createAlarmMin.ToString();
            }
        }


        //**When creating a new alarm** - decriment of the the hours button 
        private void setAlarm_decHours_Click(object sender, RoutedEventArgs e)
        {
            //if there are 0 hours in the label clicking down will change it to 12 mins
            if (createAlarmHour == 0 || createAlarmHour == 1)
            {
                createAlarmHour = 12;
                setAlarm_hours.Content = createAlarmHour.ToString();
            }
            
            //otherwise just update the current hours and update the label 
            else
            {
                createAlarmHour -= 1;
                setAlarm_hours.Content = createAlarmHour.ToString();
            }
        }

        //**When creating a new alarm** - increment of the hours button
        private void setAlarm_incHours_Click_1(object sender, RoutedEventArgs e)
        {
            //if there are 12 hours in the label clicking up will change it to 00 mins
            if (createAlarmHour == 12)
            {
                createAlarmHour = 1;
                setAlarm_hours.Content = createAlarmHour.ToString();
            }

            //if the hours are inbetween or equal to 0 and 9 then add an extra "0" to format it 
            else
            {
                createAlarmHour += 1;
                setAlarm_hours.Content = createAlarmHour.ToString();
            }
        }

        //**When creating a new alarm** - changing from "am -> pm" or "pm -> am"
        private void setAlarm_amORpm_Click(object sender, RoutedEventArgs e)
        {
            //if the button is on AM it will change to PM
            if (createAlarmAMPM == 0)
            {
                createAlarmAMPM = 1;
                setAlarm_amORpm.Content = "PM";
            }

            //otherwise the button is on PM and it will change to AM
            else
            {
                createAlarmAMPM = 0;
                setAlarm_amORpm.Content = "AM";
            }
        }

        public void setAlarm_save_Click(object sender, RoutedEventArgs e)
        {
            // ** Need to also check if it's repeating and send the last bool acordingly

            Alarm myAlarm = new Alarm(createAlarmHour, createAlarmMin, createAlarmAMPM, false);
            myAlarm.setID(idSet + 1);
            myAlarm.dismissed = false;

            //Getting the String and putting it in the linked lisst
            String temp = myAlarm.getString();
            alarms.AddLast(myAlarm);

            // Creating new User Alarm and adding it to linked list
            UserAlarm userAlarm = new UserAlarm(idSet, myAlarm);
            userAlarm.getAlarm().setRingerPath(@"C:\Users\stefan.jovanovic\Source\Repos\AlarmClock\Alarm Clock\Ringtones\Default.wav");
            userAlarm.alarm_button.Content = temp;
            userAlarm.alarm_title.Content = alarm_name.Text;

            uAlarms.AddLast(userAlarm);

            // Updating Stack Panel
            stacky.Children.Add(userAlarm);

            // Linking the user alarm to the alarm object
            myAlarm.setUserAlarm(userAlarm);
 
            slideMenuToggle(slideMenu, menuTogg);
        }

        private void setAlarm_delete_Click(object sender, RoutedEventArgs e)
        {
            /*
            // itterate through the list
            foreach (UserAlarm uAlarm in uAlarms)
            {
                if (uAlarm.getAlarm().getID() == currAlarm.getAlarm().getID())
                {
                   uAlarms.Remove(currAlarm);
                   stacky.Children.Remove(currAlarm);
                   break;
                }
            }
            */
            uAlarms.Remove(currAlarm);
            stacky.Children.Remove(currAlarm);
            slideMenuToggle(slideMenu, menuTogg);
        }


        private void alarmCheck(AlarmRing ring)
        {
            // Getting the alarm itme in "hh:mm" format
            if (uAlarms.Last != null)
            {
                foreach (UserAlarm uAlarm in uAlarms)
                {
                    String ampm = null;
                    String min = null;
                    ampm = (uAlarm.getAlarm().getAMPM() == 1 ? "PM" : "AM");
                    if (uAlarm.getAlarm().getMin().ToString().Length == 1)
                    {
                        min = "0" + uAlarm.getAlarm().getMin().ToString();
                    }
                    else
                    {
                        min = uAlarm.getAlarm().getMin().ToString();
                    }
                    String checker = uAlarm.getAlarm().getHour().ToString() + ":" + min + " " + ampm;
                    ring.compareTime(uAlarm, checker);



                }
            }
        }

        public void setCurrentAlarm(UserAlarm al)
        {
            currAlarm = al;
        }

        private void editAlarm_save_Click(object sender, RoutedEventArgs e)
        {
             currAlarm.getAlarm().setHour(createAlarmHour);
             currAlarm.getAlarm().setMin(createAlarmMin);
             currAlarm.getAlarm().setAMPM(createAlarmAMPM);
                                                  
             currAlarm.alarm_button.Content = currAlarm.getAlarm().getString();
           
             slideMenuToggle(slideMenu, menuTogg);
        }

        // Deleting the alarm
        private void dismiss_Click(object sender, RoutedEventArgs e)
        {
            this.alarmEventCanvas.Visibility = Visibility.Hidden;
        }
        public void setCurrentHour(int hour)
        {
            createAlarmHour = hour;
        }
        public void setCurrentMin(int min)
        {
            createAlarmMin = min;
        }
        public void setCurrentAMPM(int ampm)
        {
            createAlarmAMPM = ampm;
        }

        //for dissmissing the alarm
        private void dismiss1_Click(object sender, RoutedEventArgs e)
        {

            if (currAlarm != null)
            {
                if (currAlarm.getAlarm().getSnooze() == true)
                {
                    uAlarms.Remove(currAlarm);
                }
            }
            player.Stop();
            this.alertCanvas1.Visibility = Visibility.Hidden;
            this.alertCanvas2.Visibility = Visibility.Hidden;




          
            var node = uAlarms.First;
            while (node != null)
            {
                var next = node.Next;

                if (node.Value.getAlarm().getSnooze() == true)
                    uAlarms.Remove(node);
                    //testing code
                    //stacky.Children.Remove(currAlarm);

                node = next;
            }





            /*
            //check if alarm repeats itself, if it does repeate then leave alarm as is, if it doesn't repeate delete it 
            bool repeating =currAlarm.getAlarm().getRepeating();
            if(repeating == false)
            {
                 uAlarms.Remove(currAlarm);
                 stacky.Children.Remove(currAlarm);        
            }*/
        }
        public void snooze_Click(object sender, RoutedEventArgs e)
        {


            //now makes the new alarm that rings 5 mins later (this alarm is hidden from user)
            //get values of current alarm
            int newHour;
            int newMin = DateTime.Now.Minute + 1;
            //int newSec = DateTime.Now.Second + 10;
            int newAMPM;
            if (DateTime.Now.ToString("tt") == "AM")
            {
                //its am
                newAMPM = 0;
                newHour = DateTime.Now.Hour;
            }

            else
            {
                //its pm
                newAMPM = 1;
                newHour = DateTime.Now.Hour - 12;
            }


            //IF THE CURRENT ALARM IS 59MINS THE NEW MIN SHOULD BE 00 WITH HOURS BEING +1
            if(newMin >= 60)
            {
                newMin = 0;
                newHour = newHour + 1;
                //if the hour is 12 and we +1 it check if its 13, if it is then we make it == 1:00 
                if(newHour >= 13)
                {
                    newHour = 1;
                }
            }
            

            if (currAlarm != null)
            {
                if (currAlarm.getAlarm().getSnooze() == false)
                {
                    //alarm object 
                    Alarm myAlarm = new Alarm(newHour, newMin, newAMPM, false);
                    myAlarm.setID(0);
                    myAlarm.dismissed = false;
                    myAlarm.setSnooze(true);
                    //add it 
                    alarms.AddLast(myAlarm);
                    String temp = myAlarm.getString();

                    //the other alarm object
                    UserAlarm userAlarm = new UserAlarm(0, myAlarm);
                    userAlarm.getAlarm().setRingerPath(@"C:\Users\stefan.jovanovic\Source\Repos\AlarmClock\Alarm Clock\Ringtones\Default.wav");
                    userAlarm.alarm_button.Content = temp;

                    uAlarms.AddLast(userAlarm);

                    // Don't Update Stack Panel, this is for testing
                    //stacky.Children.Add(userAlarm);

                    // Linking the user alarm to the alarm object
                    myAlarm.setUserAlarm(userAlarm);
                }
                else
                {
                    uAlarms.Remove(currAlarm);
                    //alarm object 
                    Alarm myAlarm = new Alarm(newHour, newMin, newAMPM, false);
                    myAlarm.setID(0);
                    myAlarm.dismissed = false;
                    myAlarm.setSnooze(true);
                    //add it 
                    alarms.AddLast(myAlarm);
                    String temp = myAlarm.getString();

            //the other alarm object
            UserAlarm userAlarm = new UserAlarm(0, myAlarm);
            userAlarm.getAlarm().setRingerPath(@"C:\Users\stefan.jovanovic\Source\Repos\AlarmClock\Alarm Clock\Ringtones\Default.wav");
            userAlarm.alarm_button.Content = temp;

                    uAlarms.AddLast(userAlarm);

                    // Don't Update Stack Panel, this is for testing
                    stacky.Children.Add(userAlarm);

                    // Linking the user alarm to the alarm object
                    myAlarm.setUserAlarm(userAlarm);

                    //currAlarm.getAlarm().setMin(newMin);
                    //currAlarm.alarm_button.Content = currAlarm.getAlarm().getString();
                    /*
                    foreach(UserAlarm uAlarm in uAlarms)
                    {
                        if (uAlarm == currAlarm)
                        {
                            uAlarm.getAlarm().setMin(newMin);
                            currAlarm.alarm_button.Content = currAlarm.getAlarm().getString();
                        }
                    }*/
                }
                //dissmisses the inital alarm 
                player.Stop();
                this.alertCanvas1.Visibility = Visibility.Hidden;
                this.alertCanvas2.Visibility = Visibility.Hidden;




            }



        }
    }
}

