open System
open System.Security.Cryptography
open System.Text
open System.Threading

let base32alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567" |> Encoding.ASCII.GetBytes
let valToChar (b : byte) = base32alphabet.[int (b)]

module Seq =
    let groupsOfAtMost (size : int) = 
        Seq.mapi (fun i v -> i / size, i, v)
        >> Seq.groupBy (fun (g, _, _) -> g) 
        >> Seq.map (fun (_, vs) -> 
               vs
               |> Seq.sortBy(fun (_, i, _) -> i) // Ive seen no reason to believe groupby changes the order of the elements, but no reason to assume it doesnt either.
               |> Seq.map (fun (_, _, v) -> v)
               |> Seq.toList)

let base32encode (data : byte []) = 
    let leftover = data.Length % 5
    
    let cvtdata = 
        data
        |> Seq.groupsOfAtMost 5
        |> Seq.map (fun x -> 
               match x with
               | [ a; b; c; d; e ] -> (a, b, c, d, e)
               | [ a; b; c; d ] -> (a, b, c, d, 0uy)
               | [ a; b; c ] -> (a, b, c, 0uy, 0uy)
               | [ a; b ] -> (a, b, 0uy, 0uy, 0uy)
               | [ a ] -> (a, 0uy, 0uy, 0uy, 0uy)
               | _ -> (0uy, 0uy, 0uy, 0uy, 0uy))
        |> Seq.map (fun (c1, c2, c3, c4, c5) -> 
               [ valToChar (c1 >>> 3)
                 valToChar (((c1 &&& 0b111uy) <<< 2) ||| (c2 >>> 6))
                 valToChar (((c2 &&& 0b111110uy) >>> 1))
                 valToChar (((c2 &&& 0b1uy) <<< 4) ||| (c3 >>> 4))
                 valToChar (((c3 &&& 0b1111uy) <<< 1) ||| (c4 >>> 7))
                 valToChar (((c4 &&& 0b1111100uy) >>> 2))
                 valToChar (((c4 &&& 0b11uy) <<< 3) ||| (c5 >>> 5))
                 valToChar (((c5 &&& 0b11111uy))) ])
        |> Seq.concat
        |> Seq.toArray
    
    let padding = "======" |> Encoding.ASCII.GetBytes
    let cvtlen = cvtdata.Length
    match leftover with
    | 1 -> Array.blit padding 0 cvtdata (cvtlen - 7) 6
    | 2 -> Array.blit padding 0 cvtdata (cvtlen - 5) 4
    | 3 -> Array.blit padding 0 cvtdata (cvtlen - 4) 3
    | 4 -> Array.blit padding 0 cvtdata (cvtlen - 2) 1
    | _ -> ()
    Encoding.ASCII.GetString cvtdata

let truncate (data : byte []) : uint32 = 
    let offset = int ((data.[data.Length-1]) &&& 0xfuy)
    ((uint32 (data.[offset + 0] &&& 0x7fuy) <<< 24) ||| 
     (uint32 (data.[offset + 1] &&& 0xffuy) <<< 16) ||| 
     (uint32 (data.[offset + 2] &&& 0xffuy) <<< 8 ) ||| 
     (uint32 (data.[offset + 3] &&& 0xffuy))) % 1000000ul

let HMAC (K : byte []) (C : byte []) = 
    use hmac = new HMACSHA1(K)
    hmac.ComputeHash C

let counter(mutate :(uint64 -> uint64) option) = 
    uint64 (Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)) / 30UL
    |> match mutate with 
        | Some(func) -> func
        | None -> id
    |> BitConverter.GetBytes
    |> Array.rev

let gensecret() = 
    let buf = Array.zeroCreate<byte>(20)
    use rng = RNGCryptoServiceProvider.Create()
    rng.GetBytes buf
    buf

let genTOTP counter secret = (HMAC secret counter) |> truncate
let genTOTPNow secret = genTOTP (counter(None)) secret
let genTOTPAroundNow secret = seq {
     yield genTOTP (counter(None)) secret
     yield genTOTP (counter(Some(fun x -> x - 1UL))) secret
     yield genTOTP (counter(Some(fun x -> x + 1UL))) secret
     }

[<EntryPoint>]
let main _ = 
    let secret = "ASDFASDFASDFASDFSADF" |> Encoding.ASCII.GetBytes

    secret
    |> base32encode 
    |> Seq.groupsOfAtMost 3
    |> Seq.map (fun x -> String(Seq.toArray x) + " ")
    |> Seq.reduce (+)
    |> printfn "%s"

    printfn "%O\t%06d" DateTime.Now (genTOTPNow secret)

    let now = DateTime.Now
    let topOfMin = DateTime(
                    now.Year, 
                    now.Month, 
                    now.Day, 
                    now.Hour, 
                    (if (now.Second > 30) then now.Minute+1 else now.Minute), 
                    (if (now.Second > 30) then 0 else 30))

    let due = topOfMin-now
    use t = new Timer( (fun _ -> (printfn "%O\t%06d" DateTime.Now (genTOTPNow secret))), null,  due, TimeSpan.FromSeconds(30.0))
    Console.ReadKey() |> ignore
    0
