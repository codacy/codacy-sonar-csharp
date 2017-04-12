package com.codacy.dotnet

import codacy.docker.api.{Pattern, Result, Tool}
import com.codacy.dotnet.helpers.ResourceHelper
import com.github.tkqubo.html2md.Html2Markdown
import play.api.libs.json.Json

import scala.util.{Failure, Success, Try}
import scala.xml.{Node, XML}

object DocGenerator {

  case class PatternExtendedDescription(patternId: Pattern.Id, private val _extendedDescription: String) {
    val extendedDescription: String = Html2Markdown.toMarkdown(_extendedDescription)
  }

  def main(args: Array[String]): Unit = {
    Try {
      for {
        rulesFileStream <- ResourceHelper.getResourceStream("sonar-csharp-rules.xml")
        rulesXml <- Try(XML.load(rulesFileStream))
      } yield {
        (rulesXml \\ "rule").toSet.flatMap { rule: Node =>
          for {
            id <- key(rule, "key")
            title <- key(rule, "name")
            severity <- key(rule, "severity")
            description = (rule \ "description").text
            tags = (rule \\ "tag").toList.map(_.text)
            patternId = Pattern.Id(id)
            level = parseLevel(severity)
            category = selectCategory(tags)
          } yield {
            (
              Pattern.Description(patternId, Pattern.Title(title), None, None, None),
              Pattern.Specification(patternId, level, category, None, Some(Set(Pattern.Language("CSharp")))),
              PatternExtendedDescription(patternId, description)
            )
          }
        }
      }
    }.flatten match {
      case Success(rules) =>
        val (patternDescriptions, patternSpecifications, extendedDescriptions) = rules.unzip3
        val spec = Tool.Specification(Tool.Name("Sonar C#"), patternSpecifications)
        val jsonSpecifications = Json.prettyPrint(Json.toJson(spec))
        val jsonDescriptions = Json.prettyPrint(Json.toJson(patternDescriptions))


        val repoRoot = new java.io.File(".")
        val docsRoot = new java.io.File(repoRoot, "src/main/resources/docs")
        val patternsFile = new java.io.File(docsRoot, "patterns.json")
        val descriptionsRoot = new java.io.File(docsRoot, "description")
        val descriptionsFile = new java.io.File(descriptionsRoot, "description.json")

        ResourceHelper.writeFile(patternsFile.toPath, jsonSpecifications)
        ResourceHelper.writeFile(descriptionsFile.toPath, jsonDescriptions)
        extendedDescriptions.collect { case extendedDescription if extendedDescription.extendedDescription.trim.nonEmpty =>
          val descriptionsFile = new java.io.File(descriptionsRoot, s"${extendedDescription.patternId}.md")
          ResourceHelper.writeFile(descriptionsFile.toPath, extendedDescription.extendedDescription)
        }

      case Failure(e) =>
        e.printStackTrace()
    }
  }

  private def selectCategory(tags: List[String]): Pattern.Category = {
    tags
      .map(parseCategory)
      .groupBy(identity)
      .to[List]
      .sortBy { case (_, elems) => -elems.size }
      .collectFirst { case (category, _) => category }
      .getOrElse(Pattern.Category.CodeStyle)
  }

  private def parseCategory(tag: String): Pattern.Category = {
    tag match {
      case "api-design" => Pattern.Category.ErrorProne
      case "bad-practice" => Pattern.Category.ErrorProne
      case "brain-overload" => Pattern.Category.Complexity
      case "bug" => Pattern.Category.ErrorProne
      case "cert" => Pattern.Category.Security
      case "clumsy" => Pattern.Category.CodeStyle
      case "confusing" => Pattern.Category.ErrorProne
      case "convention" => Pattern.Category.CodeStyle
      case "cwe" => Pattern.Category.Security
      case "denial-of-service" => Pattern.Category.Performance
      case "design" => Pattern.Category.ErrorProne
      case "error-handling" => Pattern.Category.ErrorProne
      case "finding" => Pattern.Category.ErrorProne
      case "leak" => Pattern.Category.Security
      case "misra" => Pattern.Category.CodeStyle
      case "multi-threading" => Pattern.Category.Performance
      case "overflow" => Pattern.Category.Security
      case "owasp-a6" => Pattern.Category.Security
      case "performance" => Pattern.Category.Performance
      case "pitfall" => Pattern.Category.ErrorProne
      case "redundant" => Pattern.Category.UnusedCode
      case "sans-top25-porous" => Pattern.Category.Security
      case "sans-top25-risky" => Pattern.Category.Security
      case "security" => Pattern.Category.Security
      case "serialization" => Pattern.Category.ErrorProne
      case "suspicious" => Pattern.Category.ErrorProne
      case "unpredictable" => Pattern.Category.ErrorProne
      case "unused" => Pattern.Category.UnusedCode
      case _ => Pattern.Category.CodeStyle
    }
  }

  private def parseLevel(severity: String): Result.Level = {
    severity match {
      case "MINOR" => Result.Level.Info
      case "MAJOR" => Result.Level.Warn
      case "CRITICAL" => Result.Level.Err
      case "BLOCKER" => Result.Level.Err
      case _ => Result.Level.Info
    }
  }

  private def key(xml: Node, name: String): Set[String] = Option(xml \ name).map(_.text).filter(_.trim.nonEmpty).to[Set]

}
