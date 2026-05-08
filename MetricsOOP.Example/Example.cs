namespace MetricsOOP.Example
{
    abstract class A
    {
        private int a;

        public int b;
        public void A1() { }
        protected void A2() { }
        private void A3() { }
        internal void A4() { }
        protected internal void A5() { }
        private protected void A6() { }

        public virtual void VA1() { }
        protected virtual void VA2() { }

        public abstract void AA1();
        protected abstract void AA2();
    }

    // Level 1 (wide)
    class B : A
    {
        public override void AA1() { }
        protected override void AA2() { }

        public void B1() { }
        protected void B2() { }
        private void B3() { }

        protected override void VA2()
        {
            base.VA2();
        }
    }

    class C : A
    {
        public override void AA1() { }
        protected override void AA2() { }

        public void C1() { }
        protected void C2() { }
    }

    class D : A
    {
        public override void AA1() { }
        protected override void AA2() { }

        public void D1() { }
        private void D2() { }
        protected internal void D3() { }
    }

    // Level 2 (branching differently)
    class E : B
    {
        public override void VA1() { }

        public void E1() { }
        protected void E2() { }
    }

    class F : B
    {
        public void F1() { }
        private void F2() { }
    }

    class G : C
    {
        public void G1() { }
        protected internal void G2() { }
    }

    class H : D
    {
        public void H1() { }
        protected void H2() { }
        private protected void H3() { }
    }

    class I : D
    {
        public void I1() { }
        internal void I2() { }
    }

    class J : D
    {
        public void J1() { }
        protected void J2() { }
    }

    // Level 3 (uneven depth)
    class K : E
    {
        public void K1() { }
        protected void K2() { }
    }

    class L : F
    {
        public void L1() { }
        private void L2() { }
    }

    class M : H
    {
        public void M1() { }
        protected internal void M2() { }
    }

    class N : J
    {
        public void N1() { }
        private protected void N2() { }
    }

    class O : J
    {
        public void O1() { }
        internal void O2() { }
    }
}
