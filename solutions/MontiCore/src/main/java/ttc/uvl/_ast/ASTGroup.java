package ttc.uvl._ast;

public class ASTGroup extends ASTGroupTOP {

  public boolean isOr() {
    return this.getKind() == ASTConstantsUVL.OR;
  }
  public boolean isAlt() {
    return this.getKind() == ASTConstantsUVL.ALTERNATIVE;
  }

  public boolean isMandatory() {
    return this.getKind() == ASTConstantsUVL.MANDATORY;
  }

  public boolean isOptional() {
    return this.getKind() == ASTConstantsUVL.OPTIONAL;
  }

}
