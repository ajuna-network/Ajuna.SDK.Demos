# Ajuna.SDK.Demos [WIP]

## What is the Ajuna.SDK
Ajuna SDK is a .NET toolchain featuring .NET framework extensions and code generation utilities to build substrate storage services and clients quickly. This toolchain ideally extends [Ajuna.NetApi](https://github.com/ajuna-network/Ajuna.NetApi) library, which provides raw access to substrate nodes.

## What is the Ajuna.SDK.Demos Repository
This repository contains basic examples of how this .NET toolchain can be used. 

**IMPORTANT:** This repository is currently in a Work in Progress state since we will be constantly enriching it in the coming weeks/months.

## Demos Structure

In order to make the demos as easy to understand as possible, we have created separate Console Applications for each use case. 

The Solution has three folders:

#### Scaffolded Projects 

This folder contains the generated projects of the Ajuna.SDK:
- [SubstrateNET.NetApi](https://github.com/ajuna-network/Ajuna.SDK#ajunanetapiext)
- [SubstrateNET.RestService](https://github.com/ajuna-network/Ajuna.SDK#ajunarestservice)
- [SubstrateNET.RestClient](https://github.com/ajuna-network/Ajuna.SDK#ajunarestclient)

These projects were generated against [Substrate Node](https://github.com/paritytech/substrate) (monthly-2022-07).

#### Node Direct Access
This folder contains three demos that use our `SubstrateClientExt` (part of `SubstrateNET.NetApi`) to directly interact with the node.

- `Ajuna.SDK.Demos.DirectPolling` which shows how to directly poll the node for storage changes.
- `Ajuna.SDK.Demos.DirectSubscription` which shows how to directly subscribe to the node for storage changes.
- `Ajuna.SDK.Demos.DirectBalanceTransfer` which shows how to do a balance transfer from Alice to Bob using the `SubstrateClientExt`. 

#### Node Access via Service Layer
This folder contains two demos that use a `RestClient` (part of `SubstrateNET.RestClient`) that interacts with a `RestService` (part of `SubstrateNET.RestService`) which is responsible for communicating with the node.

- `Ajuna.SDK.Demos.ServicePolling` which shows how to poll the node for storage changes using the `RestService`.
- `Ajuna.SDK.Demos.ServiceSubscription` which shows how to directly subscribe to the node for storage changes using the `RestService`.


## How to get started 

The first and most important prerequisite is to have a running Substrate node.  

### Spin up a local substrate node
Currently you should find the most recent monthly build with a pre-generated tag in this [repo](https://github.com/paritytech/substrate), so make sure you chose a supported monthly substrate tag (ex. monthly-2022-07)

```bash
git clone -b monthly-2022-07 --single-branch https://github.com/paritytech/substrate.git
cargo build -p node-cli --release
./target/release/substrate --dev
```

### Start the RestSevice

In the _'Node Access via Service Layer'_ Demos, you will also need a running `RestService` for you `RestClient` to interact with.  

In orde to achieve this, just open a terminal within the  `SubstrateNET.RestService` folder and execute:

```bash
dotnet run
```



