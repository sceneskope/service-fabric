# service-fabric
SceneSkope Service Fabric utilities

## Various utilities for adding logging and other functionality to service fabric

To use:

Add reference to SceneSkope.ServiceFabric.XXX nuget packages

### Using Seq for logging

    var seqEventListener = SeqEventListener.Initialise();
    GC.KeepAlive(seqEventListener);

In Settings.xml in the configuration, add entries for

    <Section Name="SeqConfig">
      <Parameter Name="SeqServer" Value="-"/>
      <Parameter Name="MinimumLevel" Value="Information"/>
    </Section>

Add a parameters into the parameter section of the applicatio manifest

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

Inside a service, create an ILogger for each service, for example:

    Log = ServiceLogger.CreateLogger(this);
