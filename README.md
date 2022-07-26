# Ajuna.SDK.Demos [WIP]

## What is the Ajuna.SDK.Demos Project 

This repository contains examples of the use of Ajuna.SDK generated code. 

## How to get started 

### Spin up a local substrate node
Currently you should find for the most actual monthly build a pre-generated tag in this repo, so make sure you chose a supported monthly substrate tag (ex. monthly-2022-07)

```bash
git clone -b monthly-2022-07 --single-branch https://github.com/paritytech/substrate.git
cargo build -p node-cli --release
./target/release/substrate --dev
```




