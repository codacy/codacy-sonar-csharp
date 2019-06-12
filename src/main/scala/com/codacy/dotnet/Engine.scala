package com.codacy.dotnet

import com.codacy.tools.scala.seed.DockerEngine


object Engine extends DockerEngine(SonarCSharp)()
