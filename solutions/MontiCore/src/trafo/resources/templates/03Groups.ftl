<#-- @ftlvariable name="pm" type="de.monticore.trafo.templating.PatternMatchingAdapter"  -->
<#--We match a Feature as $F with a -->

<#--We could use 4 trafos and match the concrete syntax, but due to performance reasons we divide our matching time by 4-->
<#--We actually have to insert some abstract syntax by means of (Reference $FRef) here, as otherwise the references ID is matched-->
<#--(it also improves the performance by... a ton, as no ref->id chain has to be checked)-->
<#list pm.match("Reference $FRef <LB> <BS> Group $G [[  $_ <LB> <BS> Feature [[  Reference $IRef <LB> ]] <BE>    ]]  <BE> ", ast) as match>
    ${match['$FRef'].get().asString()} <#t>
  -> <#t>
    ${match['$IRef'].get().asString()} <#t>
    <#assign g=match['$G'].get()>
    <#-- @ftlvariable name="g" type="ttc.uvl._ast.ASTGroup"  -->
    <#if g.isAlt()>
      [arrowhead="none", arrowtail="odot", dir="forward"]<#t>
    <#elseif g.isOr()>
      [arrowhead="none", arrowtail="dot", dir="forward"]<#t>
    <#elseif g.isMandatory()>
      [arrowhead="dot", arrowtail="none", dir="forward"]<#t>
    <#elseif g.isOptional()>
      [arrowhead="odot", arrowtail="none", dir="forward"]<#t>
    </#if>
</#list>
