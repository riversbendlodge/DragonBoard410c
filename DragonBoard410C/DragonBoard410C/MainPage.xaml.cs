// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace DragonBoard
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 12;
        private GpioPin pin;
        private GpioPinValue pinValue;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private double avgTemp = 55;
        private double avgHum = 48;
        private Random rand = new Random();
        private Random randforHumidity = new Random();
        public bool blinkCommand = true;

        public MainPage()
        {
            InitializeComponent();
            
            AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            AzureIoTHub.SetBlinkCommand();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            InitGPIO();
            if (pin != null)
            {
                timer.Start();
            }        
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "Win 10 IoT Core on Arrow DragonBoard\rFive Years Out";

        }

        private void Timer_Tick(object sender, object e)
        {
            
            double currentTemp = avgTemp + rand.NextDouble() * 4 - 2;
            double currentHum = avgHum + rand.NextDouble() * 2 - 1;
            AzureIoTHub.SendDeviceToCloudMessageAsync(currentTemp, currentHum);

            var message = AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            bool blink = AzureIoTHub.GetBlinkCommand();
            
            if (blink == true)
            {

                if (pinValue == GpioPinValue.High)
                {
                    pinValue = GpioPinValue.Low;
                    pin.Write(pinValue);
                    LED.Fill = redBrush;
                }
                else
                {
                    pinValue = GpioPinValue.High;
                    pin.Write(pinValue);
                    LED.Fill = grayBrush;
                }

            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
            }


        }


    }
}
