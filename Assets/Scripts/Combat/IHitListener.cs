public interface IHitListener {
	void OnHit(AttackHitbox attack);

	void OnHitCheck(AttackHitbox attack) {}

	bool CanBeHit(AttackHitbox attack) {
		return true;
	}
}
