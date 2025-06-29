/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl._parser;

import org.antlr.v4.runtime.CommonToken;
import org.antlr.v4.runtime.CommonTokenStream;

import de.monticore.parser.whitespace.WhitespacePreprocessingTokenSource;
import org.antlr.v4.runtime.Lexer;

import java.io.FileReader;
import java.io.IOException;
import java.io.Reader;
import java.nio.charset.StandardCharsets;

/**
 * Add support
 */
public class UVLParser extends UVLParserTOP {

  public static final String LINEBREAK = "<LB>";
  public static final String BLOCK_START = "<BS>";
  public static final String BLOCK_END = "<BE>";

  @SuppressWarnings("deprecation") // parsers are marked as deprecated to avoid direct usage
  public UVLParser() {
  }

  public CommonTokenStream getPreprocessedTokenStream(Lexer lexer) {
    WhitespacePreprocessingTokenSource tokensWithPreprocessing = new WhitespacePreprocessingTokenSource(
            lexer,
            new CommonToken(UVLAntlrLexer.LINEBREAK, LINEBREAK),
            new CommonToken(UVLAntlrLexer.BLOCK_START, BLOCK_START),
            new CommonToken(UVLAntlrLexer.BLOCK_END, BLOCK_END),
            -1
    );
    // Use CommonTokenStream to hide the hidden channel again
    return new CommonTokenStream(tokensWithPreprocessing);
  }


  @Override
  protected UVLAntlrParser create(Reader reader) throws IOException {
    UVLAntlrLexer lexer = new UVLAntlrLexer(org.antlr.v4.runtime.CharStreams.fromReader(reader));

    // Use CommonTokenStream to hide the hidden channel again
    UVLAntlrParser parser = new UVLAntlrParser(getPreprocessedTokenStream(lexer));
    lexer.setMCParser(parser);
    lexer.removeErrorListeners();
    lexer.addErrorListener(new de.monticore.antlr4.MCErrorListener(parser));
    parser.setFilename("StringReader");
    setError(false);
    return parser;
  }

  @Override
  protected UVLAntlrParser create(String fileName) throws IOException {
    UVLAntlrLexer lexer = new UVLAntlrLexer(org.antlr.v4.runtime.CharStreams.fromReader(
            new FileReader(fileName, StandardCharsets.UTF_8)));

    UVLAntlrParser parser = new UVLAntlrParser(getPreprocessedTokenStream(lexer));
    lexer.setMCParser(parser);
    lexer.removeErrorListeners();
    lexer.addErrorListener(new de.monticore.antlr4.MCErrorListener(parser));
    parser.setFilename(fileName);

    setError(false);
    return parser;
  }

}
