package com.codacy.dotnet

import java.io._
import java.nio.file.{Files, Path}

import codacy.docker.api._
import codacy.dockerApi.utils.CommandRunner
import com.codacy.dotnet.helpers.ResourceHelper
import com.codacy.dotnet.protobuf.SonarAnalyzer

import scala.annotation.tailrec
import scala.collection.JavaConversions._
import scala.util.{Failure, Properties, Try}

object SonarCSharp extends Tool {

  override def apply(source: Source.Directory, configuration: Option[List[Pattern.Definition]],
                     files: Option[Set[Source.File]])(implicit specification: Tool.Specification): Try[List[Result]] = {
    Try {
      getConfig(source, configuration, files).flatMap {
        configPath =>
          val outputDir = Files.createTempDirectory("sonar-csharp-output").toAbsolutePath
          val cmd = getCmd(configPath, outputDir)

          CommandRunner.exec(cmd, Some(new File(source.path))) match {
            case Right(res) if res.exitCode == 0 => loadIssues(outputDir)
            case Right(res) => Failure(new Exception(
              s"""
                 |Failed to run Sonar C#
                 |${res.stdout.mkString(Properties.lineSeparator)}
                 |${res.stderr.mkString(Properties.lineSeparator)}
              """.stripMargin
            ))
            case Left(e) => Failure(e)
          }
      }.map { issues =>
        for {
          fileIssues <- issues
          fileIssue <- fileIssues.getIssueList.toList
        } yield {
          Result.Issue(Source.File(fileIssues.getFilePath), Result.Message(fileIssue.getMessage),
            Pattern.Id(fileIssue.getId), Source.Line(fileIssue.getLocation.getStartLine))
        }
      }
    }.flatten
  }

  private def getCmd(configPath: Path, outputDir: Path): List[String] = {
    List("mono", "/opt/docker/sonar-csharp/SonarAnalyzer.Scanner.exe", configPath.toString, outputDir.toString, "cs")
  }

  private def getConfig(source: Source.Directory, configuration: Option[List[Pattern.Definition]], files: Option[Set[Source.File]]): Try[Path] = {
    Try {
      val tmpDir = Files.createTempDirectory("sonar-csharp-config").toFile
      val tmpFile = new File(tmpDir, "SonarLint.xml").toPath.toAbsolutePath
      val xml = generateXml(source, configuration, files)
      ResourceHelper.writeFile(tmpFile, xml)
      tmpFile
    }
  }

  private def generateXml(source: Source.Directory, configuration: Option[List[Pattern.Definition]], files: Option[Set[Source.File]]): String = {
    s"""<?xml version="1.0" encoding="UTF-8"?>
       |<AnalysisInput>
       |  <Settings>
       |    <Setting>
       |      <Key>sonar.cs.ignoreHeaderComments</Key>
       |      <Value>true</Value>
       |    </Setting>
       |  </Settings>
       |  <Rules>
       |    ${generateRules(configuration).mkString(Properties.lineSeparator)}
       |  </Rules>
       |  <Files>
       |    ${generateFiles(source, files).mkString(Properties.lineSeparator)}
       |  </Files>
       |</AnalysisInput>
     """.stripMargin
  }

  private def generateRules(configuration: Option[List[Pattern.Definition]]): List[String] = {
    for {
      pattern <- configuration.getOrElse(List.empty)
    } yield {
      val parametersXml = pattern.parameters.map { parameters =>
        val parametersXml = parameters.map { parameter =>
          val paramKeyXml = s"<Key>${parameter.name}</Key>"
          val paramValueXml = s"<Value>${parameter.value}</Value>"

          s"""<Parameter>
             |    ${paramKeyXml}
             |    ${paramValueXml}
             |</Parameter>""".stripMargin
        }

        s"""<Parameters>
           |    ${parametersXml.mkString(Properties.lineSeparator)}
           |</Parameters>""".stripMargin
      }.to[List]

      val keyXml = s"<Key>${pattern.patternId.value}</Key>"

      s"""<Rule>
         |    ${keyXml}
         |    ${parametersXml.mkString(Properties.lineSeparator)}
         |</Rule>
       """.stripMargin
    }
  }

  private def generateFiles(source: Source.Directory, filesOpt: Option[Set[Source.File]]): Set[String] = {
    for {
      file <- filesOpt.getOrElse(Set.empty)
    } yield {
      s"<File>${file.path.stripPrefix(source.path).stripPrefix("/")}</File>"
    }
  }

  private def loadIssues(outputDir: Path): Try[List[SonarAnalyzer.FileIssues]] = {
    val inputStream = new FileInputStream(new File(s"$outputDir/issues.pb"))
    val issues = Try(recursiveAccomulator(inputStream, List.empty))

    if (inputStream != null) inputStream.close()

    issues
  }

  @tailrec
  private def recursiveAccomulator(inputStream: InputStream, issues: List[SonarAnalyzer.FileIssues]): List[SonarAnalyzer.FileIssues] = {
    Option(SonarAnalyzer.FileIssues.parseDelimitedFrom(inputStream)) match {
      case None => issues
      case Some(message) => recursiveAccomulator(inputStream, issues :+ message)
    }
  }

}
