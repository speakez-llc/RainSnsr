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
    
    let toggleRelay (relay : Relay) (message : string) =
        async {
            Resolver.Log.Info(message)
            relay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            relay.Toggle()
        }
    
    let ShowColor (color : Color) (duration : TimeSpan) = async {
        led.SetColor(color, 15.0f) 
        do! Async.Sleep duration
        led.SetBrightness(0.0f)
    }

    let getRainState : Async<bool> = async {
        if rainSensor.State then
            Resolver.Log.Info("It's raining!")
            return true
        else
            Resolver.Log.Info("No Rain!")
            return false
    }
    
    let shakeRainSnsr = async {
        let stopwatch = System.Diagnostics.Stopwatch()
        stopwatch.Start()
        while stopwatch.Elapsed.TotalSeconds < 2.0 do
            do! wiperServo.RotateTo(Angle 155) |> Async.AwaitTask
            do! wiperServo.RotateTo(Angle 180) |> Async.AwaitTask
        do! Task.Delay(1000) |> Async.AwaitTask
    }
        
    let runRainSnsrAsync = async {
        do! wiperServo.RotateTo(Angle(0.0, Angle.UnitType.Degrees)) |> Async.AwaitTask
        Resolver.Log.Info "Starting Rain Sensor..."
        while true do
            let rainState = getRainState |> Async.RunSynchronously
            if rainState then
                do! toggleRelay retractRelay "Retracting awning..."
                do! ShowColor Color.Aqua (TimeSpan.FromMilliseconds 500) 
                do! wiperServo.RotateTo(Angle 160) |> Async.AwaitTask
                shakeRainSnsr |> Async.RunSynchronously
                do! wiperServo.RotateTo(Angle 0) |> Async.AwaitTask          
                do! Task.Delay(5000) |> Async.AwaitTask
            else 
                do! Task.Delay(15000) |> Async.AwaitTask
    }

    override this.Initialize() =
        Resolver.Log.Info "Initialize..."
        let servoConfig = ServoConfig(
            minimumAngle = Angle 0,
            maximumAngle = Angle 180
            )
        retractRelay <- Relay(MeadowApp.Device.Pins.D09)
        stopRelay <- Relay(MeadowApp.Device.Pins.D08)
        extendRelay <- Relay(MeadowApp.Device.Pins.D07)
        lightRelay <- Relay(MeadowApp.Device.Pins.D06)
        p2relay <- Relay(MeadowApp.Device.Pins.D05)
        rainSensor <- MeadowApp.Device.Pins.D13.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        wiperServo <- Servo(MeadowApp.Device.Pins.D12, servoConfig)
        led <- new RgbPwmLed(
            MeadowApp.Device.Pins.D02,
            MeadowApp.Device.Pins.D03, 
            MeadowApp.Device.Pins.D04)

        Task.CompletedTask;

    override this.Run () =
        task {
            do! runRainSnsrAsync
        } 