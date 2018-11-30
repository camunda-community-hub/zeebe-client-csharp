#!/bin/bash

./packages/Grpc.Tools.1.16.0/tools/windows_x64/protoc  -Igateway-protocol/csharp-protocol/target/proto   --csharp_out Client/Impl/proto   --grpc_out Client/Impl/proto   gateway-protocol/csharp-protocol/target/proto/gateway.proto   --plugin="protoc-gen-grpc=./packages/Grpc.Tools.1.16.0/tools/windows_x64/grpc_csharp_plugin.exe"


