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
}
