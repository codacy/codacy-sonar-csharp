package com.codacy.dotnet.helpers

import java.io.InputStream
import java.nio.charset.{CodingErrorAction, StandardCharsets}
import java.nio.file.{Files, Path, StandardOpenOption}

import scala.io.Codec
import scala.util.{Failure, Try}

object ResourceHelper {

  implicit val codec = Codec("UTF-8")
  codec.onMalformedInput(CodingErrorAction.REPLACE)
  codec.onUnmappableCharacter(CodingErrorAction.REPLACE)

  def getResourceStream(path: String): Try[InputStream] = {
    Option(getClass.getClassLoader.getResource(path)).map { url =>
      Try(url.openStream())
    }.getOrElse {
      Failure(new Exception("The path provided was not found"))
    }
  }

  def writeFile(path: Path, content: String): Try[Path] = {
    Try(Files.write(
      path,
      content.getBytes(StandardCharsets.UTF_8),
      StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING, StandardOpenOption.WRITE
    ))
  }

}
