namespace RainSnsr

open System
open Meadow
open Meadow.Devices
open Meadow.Foundation.Leds
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open System.Threading.Tasks
open Meadow.Units

type MeadowApp() =
    inherit App<F7FeatherV1>()

    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable lightRelay : Relay = null
    let mutable p2relay : Relay = null
    let mutable rainSensor : IDigitalInputPort = null
    let mutable wiperServo : Servo = null

    let mutable led : RgbPwmLed = null
    
    let retractAwning =
        async {
            Resolver.Log.Info("Toggling Retract Relay...")
            retractRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            retractRelay.Toggle()
        }
            
    let expandAwning =
        async {
            Resolver.Log.Info("Toggling Extend Relay...")
            extendRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            extendRelay.Toggle()
        }
            
    let stopAwning =
        async {
            Resolver.Log.Info("Toggling Stop Relay...")
            stopRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            stopRelay.Toggle()
        }
        
    let toggleLight =
        async {
            Resolver.Log.Info("Toggling Light Relay...")
            lightRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            lightRelay.Toggle()
        }
        
    let toggleP2 =
        async {
            Resolver.Log.Info("Toggling P2 Relay...")
            p2relay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            p2relay.Toggle()
        }

    let ShowColor (color : Color) (duration : TimeSpan) = async {
        led.SetColor(color, 15.0f) 
        do! Async.Sleep duration
    }

    let StopLED = async {
        led.SetBrightness(0.0f) 
        do! Async.Sleep(TimeSpan.FromMilliseconds(1000.0))
    }

    let CycleColors (duration : TimeSpan) = async {
        do Resolver.Log.Info "Cycle colors..."

        while true do
            do! ShowColor Color.Red duration 
            do! StopLED
            do! ShowColor Color.Blue duration
            do! StopLED
            do! ShowColor Color.Green duration
            do! StopLED
            do! ShowColor Color.Aqua duration 
            do! StopLED
            do! ShowColor Color.Gold duration
            do! StopLED
            do! ShowColor Color.Magenta duration
            do! StopLED
            do! ShowColor Color.White duration
            do! StopLED
    }
    let runServoAsync = async {
        Resolver.Log.Info "Initializing Servo..."
        do! wiperServo.RotateTo(Angle(0.0, Angle.UnitType.Degrees)) |> Async.AwaitTask
        Resolver.Log.Info "Running Servo..."
        while true do
            do! wiperServo.RotateTo(wiperServo.Config.MaximumAngle) |> Async.AwaitTask
            do! Task.Delay(1000) |> Async.AwaitTask
            do! wiperServo.RotateTo(wiperServo.Config.MinimumAngle) |> Async.AwaitTask          
            do! Task.Delay(1000) |> Async.AwaitTask
    }

    override this.Initialize() =
        Resolver.Log.Info "Creating Outputs"
        let servoConfig = ServoConfig(
            maximumAngle = Angle(90, Angle.UnitType.Degrees),
            minimumPulseDuration = 650,
            maximumPulseDuration = 1300
        )
        Resolver.Log.Info "Creating Relay Outputs"
        retractRelay <- Relay(MeadowApp.Device.Pins.D09)
        stopRelay <- Relay(MeadowApp.Device.Pins.D08)
        extendRelay <- Relay(MeadowApp.Device.Pins.D07)
        lightRelay <- Relay(MeadowApp.Device.Pins.D06)
        p2relay <- Relay(MeadowApp.Device.Pins.D05)
        Resolver.Log.Info "Creating Relay Input"
        rainSensor <- MeadowApp.Device.Pins.D13.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        Resolver.Log.Info "Creating Servo Output"
        wiperServo <- Servo(MeadowApp.Device.Pins.D12, servoConfig)
        
        Resolver.Log.Info "Initialize LED..."

        led <- new RgbPwmLed(
            MeadowApp.Device.Pins.D02,
            MeadowApp.Device.Pins.D03, 
            MeadowApp.Device.Pins.D04)

        Task.CompletedTask;

    override this.Run () =
        task {
            do Resolver.Log.Info "Run LED..."
            do! CycleColors(TimeSpan.FromSeconds(1.0))
        } |> ignore
        task {
            do Resolver.Log.Info "Run Servo..."
            do! runServoAsync
        } |> ignore
        task {
            do Resolver.Log.Info "Run Each Relay..."
            while true do
                Resolver.Log.Info("tiggering relays")
                do! retractAwning
                do! Task.Delay(1000)
                do! stopAwning
                do! Task.Delay(1000)
                do! expandAwning
                do! Task.Delay(1000)
                do! toggleLight
                do! Task.Delay(1000)
                do! toggleP2
                do! Task.Delay(1000)
        } |> ignore
        task {
            do Resolver.Log.Info "Run Rain Sensor..."
            while true do
                let rain = rainSensor.State
                if rain then
                    Resolver.Log.Info("It's raining!")
                else
                    Resolver.Log.Info("It's not raining!")
                do! Task.Delay(1000)
        }