package ttc.uvl._parser;

import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.Pair;

import java.util.*;

public class WhitespacePreprocessingTokenSource implements TokenSource {
  protected final Lexer delegate;
  protected final Token eolTokenProto;
  protected final Token incIndentTokenProto;
  protected final Token decIndentTokenProto;
  protected final int continueLineTokenType;
  protected final List<String> closingParens = List.of(")", "}", "]");
  protected final List<String> openingParens = List.of("(", "{", "[");
  protected int lastLine = 1;
  protected int lastIndent = 0;
  protected Token lastToken;
  protected Pair<TokenSource, CharStream> source;
  protected Set<Token> alreadyProcessedEol = new HashSet<>();
  protected Set<Token> alreadyProcessedParen = new HashSet<>();
  protected Set<Token> alreadyProcessedBlockStart = new HashSet<>();
  protected Set<Token> alreadyProcessedBlockEnd = new HashSet<>();
  protected LinkedList<Token> queue = new LinkedList<>();
  protected int indentDepth = 0;
  protected int parenDepth = 0;

  public WhitespacePreprocessingTokenSource(Lexer delegate, Token eolTokenProto, Token incIndentTokenProto, Token decIndentTokenProto, int continueLineTokenType) {
    this.delegate = delegate;
    this.eolTokenProto = eolTokenProto;
    this.incIndentTokenProto = incIndentTokenProto;
    this.decIndentTokenProto = decIndentTokenProto;
    this.continueLineTokenType = continueLineTokenType;
    this.source = new Pair<>(this, delegate.getInputStream());
  }

  protected boolean isWhitespaceSensitive() {
    return parenDepth == 0;
  }

  @Override
  public Token nextToken() {
    Token res = null;

    if (!queue.isEmpty()) {
      res = queue.peek();
    }

    if (res == null || mustBeProcessed(res)) {
      Token token;
      if (res == null) {
        token = delegate.nextToken();
        if (token.getType() == continueLineTokenType) {
          lastLine++;
          token = delegate.nextToken();
        }

        queue.add(0, token);
        res = token;
      } else {
        token = res;
      }

      if (isClosingParen(token)) {
        parenDepth--;
      }

      if (isWhitespaceSensitive()) {
        if (indentIncreased(token) && token.getType() != Token.EOF) { // not for EOF=-1
          indentDepth++;
          indentStops.push(getIndent(token));
          alreadyProcessedBlockStart.add(token);
          queue.add(0, createTokenWithLastChannel(incIndentTokenProto)); // make sure the previous channel is used here
        } else if (indentDecreased(token)) {
          int curIndent = getIndent(token);
          while ((curIndent == 0 && !indentStops.isEmpty()) || (!indentStops.isEmpty() && indentStops.peek() > curIndent)) {
            indentDepth--;
            queue.add(0, createToken(decIndentTokenProto));
            indentStops.pop();
          }

          alreadyProcessedBlockEnd.add(token);
        }
      }
      if (needsEol(token)) {
        alreadyProcessedEol.add(token);
        queue.add(0, createToken(eolTokenProto));

      }
      // EOF -> all blocks are automatically closed
      if (token.getType() == -1) {
        int insertPos = 0;
        for (int i = 0; i < queue.size(); i++) {
          if (queue.get(i).getType() == -1) {
            insertPos = i;
            break;
          }
        }

        for (int i = 0; i < indentDepth; i++) {
          queue.add(insertPos, createToken(decIndentTokenProto));
        }
      }


      if (isOpeningParen(token) && !alreadyProcessedParen.contains(token)) {
        parenDepth++;
        alreadyProcessedParen.add(token);
      }
    }

    if (!queue.isEmpty()) {
      res = queue.poll();
    }

    if (isNewLine(res) && parenDepth == 0 && !isClosingParen(res) || isNewLine(res) && parenDepth == 1 && isOpeningParen(res)) {
      lastIndent = getIndent(res);
    }
    lastLine = res.getLine();
    lastToken = res;
    alreadyProcessedEol.add(res);
    return res;
  }

  protected boolean mustBeProcessed(Token t) {
    return !alreadyProcessedEol.contains(t) || alreadyProcessedBlockEnd.contains(t);
  }

  protected Token createToken(Token proto) {
    int stopIndex = lastToken != null ? lastToken.getStopIndex() : 0;
    int charPositionInLine = lastToken != null ? lastToken.getCharPositionInLine() + lastToken.getText().length() : 0;
    return getTokenFactory().create(source, proto.getType(), proto.getText(), delegate.getChannel(), stopIndex, stopIndex + 1, lastLine, charPositionInLine);
  }

  protected Token createTokenOnDefault(Token proto) {
    int stopIndex = lastToken != null ? lastToken.getStopIndex() : 0;
    int charPositionInLine = lastToken != null ? lastToken.getCharPositionInLine() + lastToken.getText().length() : 0;
    return getTokenFactory().create(source, proto.getType(), proto.getText(), Token.DEFAULT_CHANNEL, stopIndex, stopIndex + 1, lastLine, charPositionInLine);
  }

  protected Token createTokenWithLastChannel(Token proto) {
    int stopIndex = lastToken != null ? lastToken.getStopIndex() : 0;
    int charPositionInLine = lastToken != null ? lastToken.getCharPositionInLine() + lastToken.getText().length() : 0;
    return getTokenFactory().create(source, proto.getType(), proto.getText(), lastToken == null ? delegate.getChannel() : lastToken.getChannel(), stopIndex, stopIndex + 1, lastLine, charPositionInLine);
  }

  protected boolean needsEol(Token token) {
    if (parenDepth > 0) {
      return false;
    }

    if (alreadyProcessedEol.contains(token)) {
      return false;
    }

    if (lastToken != null && (lastToken.getText().equals(":") || lastToken.getType() == eolTokenProto.getType() || lastToken.getType() == incIndentTokenProto.getType() || lastToken.getType() == decIndentTokenProto.getType())) {
      return false;
    }
    return token.getType() == -1 || isNewLine(token);
  }

  protected boolean isNewLine(Token token) {
    return lastLine != token.getLine();
  }

  protected boolean indentIncreased(Token token) {
    if (alreadyProcessedBlockStart.contains(token)) {
      return false;
    }

    return isNewLine(token) && getIndent(token) > lastIndent;
  }

  protected Stack<Integer> indentStops = new Stack<>();

  protected boolean indentDecreased(Token token) {
    if (alreadyProcessedBlockEnd.contains(token)) {
      return false;
    }

    return isNewLine(token) && getIndent(token) < lastIndent;
  }

  protected int getIndent(Token token) {
    return token.getCharPositionInLine();
  }

  protected boolean isClosingParen(Token token) {
    return closingParens.contains(token.getText());
  }

  protected boolean isOpeningParen(Token token) {
    return openingParens.contains(token.getText());
  }

  @Override
  public int getLine() {
    if (!queue.isEmpty()) {
      return queue.peek().getLine();
    }

    return delegate.getLine();
  }

  @Override
  public int getCharPositionInLine() {
    if (!queue.isEmpty()) {
      return queue.peek().getCharPositionInLine();
    }

    return delegate.getCharPositionInLine();
  }

  @Override
  public CharStream getInputStream() {
    return delegate.getInputStream();
  }

  @Override
  public String getSourceName() {
    return delegate.getSourceName();
  }

  @Override
  public TokenFactory<?> getTokenFactory() {
    return delegate.getTokenFactory();
  }

  @Override
  public void setTokenFactory(TokenFactory<?> factory) {
    delegate.setTokenFactory(factory);
  }
}
