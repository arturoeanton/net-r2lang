function f1(a){
    println("a:",a)
}
function main(){
    r2(f1,1) 
    r2_wait_all()
    r2(f1,2)
    r2_wait_all()
     r2(f1,3)
    r2_wait_all()
      r2(f1,4)

}