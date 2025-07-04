/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl._ast;

public class ASTGroup extends ASTGroupTOP {

  // Add some helper functions to expose the constants to FTL
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
