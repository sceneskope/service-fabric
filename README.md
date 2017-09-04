# service-fabric
Service Fabric utilities

## Various utilities for adding logging and other functionality to service fabric

[![Build status](https://ci.appveyor.com/api/projects/status/nt97j90bbfeu40q0?svg=true)](https://ci.appveyor.com/api/projects/status/nt97j90bbfeu40q0/branch/master?svg=true)

[![NuGet](https://img.shields.io/nuget/v/SceneSkope.Utilities.svg)](https://www.nuget.org/packages/ServiceFabric.Utilities/)
[![MyGet CI](https://img.shields.io/myget/sceneskope-ci/v/ServiceFabric.Utilities.svg)](http://myget.org/gallery/sceneskope-ci)

To use:

Add reference to ServiceFabric.XXX nuget packages

### Using Seq for logging

    SerilogEventListener.Initialise(SeqEventLogger.DefaultLogger);
    GC.KeepAlive(seqEventListener);

In Settings.xml in the configuration, add entries for

    <Section Name="SeqConfig">
      <Parameter Name="SeqServer" Value="" MustOverride=true />
      <Parameter Name="ApiKey" Value="-" />
      <Parameter Name="MinimumLevel" Value="Information"/>
    </Section>

Add a parameters into the parameter section of the application manifest

    <Parameter Name="SeqServer" DefaultValue="" />

In the application manifest, for each service with Seq logging

    <ConfigOverrides>
      <ConfigOverride Name="Config">
        <Settings>
          <Section Name="SeqConfig">
            <Parameter Name="SeqServer" Value="[SeqServer]" />
          </Section>
        </Settings>
      </ConfigOverride>
    </ConfigOverrides>


In the application parameters configure the parameter

    <Parameter Name="SeqServer" Value="http://localhost:5341" />


## Getting partition Information
Add reference to ServiceFabric.utilities

To list the partitions for a service:

    PartitionUtilities.GetPartitionListAsync(serviceName);

To list the partitions ordered, useful if you want to show partition N of MinimumLevel

    PartitionUtilities.GetOrderedPartitionListAsync(serivceName);

Extension method to get the service partition key for a partition:

    partitionInformation.ServicePartitionKey();

To get a list of services across all partitions:

    partitionInformation.GetServiceListAsync<ISampleService>(serviceName);

