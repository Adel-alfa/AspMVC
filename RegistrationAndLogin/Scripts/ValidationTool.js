///
// tool for validation string name

function IsAlphaExp(str) {
    var justAlph = /^[a-zA-Z]+$/;
    return justAlph.test(str);
}