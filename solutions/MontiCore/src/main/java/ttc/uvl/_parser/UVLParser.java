/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl._parser;

import org.antlr.v4.runtime.BufferedTokenStream;
import org.antlr.v4.runtime.CommonToken;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.DiagnosticErrorListener;
import org.antlr.v4.runtime.atn.PredictionMode;

import java.io.FileReader;
import java.io.IOException;
import java.io.Reader;
import java.nio.charset.StandardCharsets;

/**
 * Add support
 */
public class UVLParser extends UVLParserTOP {
  public BufferedTokenStream currentTokenStream;
  public UVLAntlrParser currentParser;
  public boolean debugPerformance = false;

  public static final String LINEBREAK = "<LB>";
  public static final String BLOCK_START = "<BS>";
  public static final String BLOCK_END = "<BE>";

  @SuppressWarnings("deprecation") // parsers are marked as deprecated to avoid direct usage
  public UVLParser() {
  }

  @Override
  protected UVLAntlrParser create(Reader reader) throws IOException {
    UVLAntlrLexer lexer = new UVLAntlrLexer(org.antlr.v4.runtime.CharStreams.fromReader(reader));

    WhitespacePreprocessingTokenSource tokensWithPreprocessing = new WhitespacePreprocessingTokenSource(
            lexer,
            new CommonToken(UVLAntlrLexer.LINEBREAK, LINEBREAK),
            new CommonToken(UVLAntlrLexer.BLOCK_START, BLOCK_START),
            new CommonToken(UVLAntlrLexer.BLOCK_END, BLOCK_END),
            -1
    );
    // Use CommonTokenStream to hide the hidden channel again
    BufferedTokenStream stream =  new CommonTokenStream(tokensWithPreprocessing);
    currentTokenStream = stream;
    UVLAntlrParser parser = new UVLAntlrParser(stream);
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

    WhitespacePreprocessingTokenSource tokensWithPreprocessing = new WhitespacePreprocessingTokenSource(
            lexer,
            new CommonToken(UVLAntlrLexer.LINEBREAK, LINEBREAK),
            new CommonToken(UVLAntlrLexer.BLOCK_START, BLOCK_START),
            new CommonToken(UVLAntlrLexer.BLOCK_END, BLOCK_END),
            -1
    );
    // Use CommonTokenStream
    BufferedTokenStream stream = new CommonTokenStream(tokensWithPreprocessing);
    currentTokenStream = stream;
    UVLAntlrParser parser = new UVLAntlrParser(stream);
    currentParser = parser;
    lexer.setMCParser(parser);
    lexer.removeErrorListeners();
    lexer.addErrorListener(new de.monticore.antlr4.MCErrorListener(parser));
    parser.setFilename(fileName);

    if (debugPerformance) {
      // TODO: Tmp for performance analysis
      parser.addErrorListener(new DiagnosticErrorListener());
      parser.getInterpreter().setPredictionMode(PredictionMode.LL_EXACT_AMBIG_DETECTION);
      parser.setProfile(true);
    }

    setError(false);
    return parser;
  }

}
