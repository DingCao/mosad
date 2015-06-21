#ifndef __HELLOWORLD_SCENE_H__
#define __HELLOWORLD_SCENE_H__

#include "cocos2d.h"
USING_NS_CC;

class HelloWorld : public cocos2d::Layer
{
public:
    // there's no 'id' in cpp, so we recommend returning the class instance pointer
    static cocos2d::Scene* createScene();

    // Here's a difference. Method 'init' in cocos2d-x returns bool, instead of returning 'id' in cocos2d-iphone
    virtual bool init();
    
    // a selector callback
    void menuCloseCallback(cocos2d::Ref* pSender);
    
    // implement the "static create()" method manually
    CREATE_FUNC(HelloWorld);
     HelloWorld();
	 ~HelloWorld();


	// ���ּ����벥��
	void preloadMusic();
	void playBgm();
	void playEffect();
	void actionCallBack();


	void testTouchEvent();

	void testKeyboardEvent();

	void testMouseEvent();

	void testCustomEvent();

	void testAccelerationEvent();

// public:


private:
	EventDispatcher* dispatcher;

	MotionStreak* streak;

	Vec2 touch_pos;

	bool isTouching;
	bool isCut;

	Sprite* rope;
	Sprite* man;
	Sprite* box;
};

#endif // __HELLOWORLD_SCENE_H__
