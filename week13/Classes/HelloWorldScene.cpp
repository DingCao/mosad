#include "HelloWorldScene.h"
#include <iostream>
#include <string>
using namespace std;
#include "SimpleAudioEngine.h"

#pragma execution_character_set("utf-8")

using namespace CocosDenshion;
USING_NS_CC;

Scene* HelloWorld::createScene()
{
    // 'scene' is an autorelease object
    auto scene = Scene::create();
	
    
    // 'layer' is an autorelease object
    auto layer = HelloWorld::create();

    // add layer as a child to scene
    scene->addChild(layer);

    // return the scene
    return scene;
}

HelloWorld::HelloWorld()
{

}
HelloWorld::~HelloWorld()
{

}

// on "init" you need to initialize your instance
bool HelloWorld::init()
{
    //////////////////////////////
    // 1. super init first
    if ( !Layer::init() )
    {
        return false;
    }
    
	Size visibleSize = Director::getInstance()->getVisibleSize();
	Vec2 origin = Director::getInstance()->getVisibleOrigin();

	dispatcher = Director::getInstance()->getEventDispatcher();

	preloadMusic();
	playBgm();

	//此demo中自定义事件由触摸事件onTouchBegan触发
	testTouchEvent();
	testCustomEvent();


	testKeyboardEvent();
	// testMouseEvent();

	testAccelerationEvent();

	//testDemo();

	//添加精灵绳子
	rope = Sprite::create("Demo/rope.png");
	rope->setPosition(Vec2(visibleSize.width / 2, visibleSize.height - rope->getContentSize().height / 4));
	addChild(rope);

	//添加精灵人
	man = Sprite::create("Demo/man.png");
  man->setPosition(Vec2(visibleSize.width / 2, man->getContentSize().height / 2));
	addChild(man);

	//添加精灵箱子
	box = Sprite::create("Demo/box.png");
  box->setPosition(Vec2(visibleSize.width / 2, visibleSize.height - rope->getContentSize().height / 2 - box->getContentSize().height));
	addChild(box);


	// isCut = false;
	isTouching = false;
	return true;
}


void HelloWorld::menuCloseCallback(Ref* pSender)
{

    Director::getInstance()->end();

#if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif
}

void HelloWorld::preloadMusic()
{
	SimpleAudioEngine::getInstance()->preloadBackgroundMusic("music/bgm.mp3");
	SimpleAudioEngine::getInstance()->preloadEffect("music/shoot.mp3");
}

void HelloWorld::playBgm()
{
	SimpleAudioEngine::getInstance()->playBackgroundMusic("music/bgm.mp3", true);
}

void HelloWorld::playEffect()
{
	SimpleAudioEngine::getInstance()->playEffect("music/shoot.mp3");
}



void HelloWorld::testTouchEvent()
{

  isCut = false;  // 初始化为没有切割
	streak = MotionStreak::create(0.5f, 10, 30, Color3B::WHITE, "Demo/flash.png");
	this->addChild(streak, 2);

	auto listener = EventListenerTouchOneByOne::create();

	listener->onTouchBegan = [&](Touch* touch, Event* event){
		touch_pos = touch->getLocation();

		EventCustom e("test");

		e.setUserData(&touch_pos);

		dispatcher->dispatchEvent(&e);

		return true;
	};

	listener->onTouchMoved = [&](Touch* touch, Event* event){
		touch_pos = touch->getLocation();
		//滑动拖尾效果

    // 判断是否切割
		if (touch_pos.x > rope->getPositionX() - rope->getContentSize().width / 2 &&
			touch_pos.x < rope->getPositionX() + rope->getContentSize().width / 2 &&
			touch_pos.y > rope->getPositionY() - rope->getContentSize().height / 2 &&
			touch_pos.y < rope->getPositionY() + rope->getContentSize().height / 2 ) {
			isCut = true;
		}
    
		streak->setPosition(touch_pos);
	};

	listener->onTouchEnded = [&](Touch* touch, Event* event){
		log("onTouchEnded");
    playEffect();

    // 切割后特效
		if (isCut) {
			auto action2 = FadeOut::create(1.0f);
			rope->runAction(action2);
			Vec2 toLocation = Vec2(man->getPosition().x, man->getPosition().y*3);

			auto moveTo = MoveTo::create(1.0f, toLocation);
			box->runAction(moveTo);
		}

    // 无论是否切割绳子 回调一次
    auto callBack = CallFunc::create(CC_CALLBACK_0(HelloWorld::actionCallBack, this));
    this->runAction(callBack);
	};

	dispatcher->addEventListenerWithSceneGraphPriority(listener, this);
}

void HelloWorld::actionCallBack()
{
	if (box->getPositionY() == man->getPosition().y * 3) {
		isTouching = true;
	}

	if (isTouching == true) {
		SimpleAudioEngine::getInstance()->stopBackgroundMusic("music/bgm.mp3");
	}
}


void HelloWorld::testAccelerationEvent()
{
	Device::setAccelerometerEnabled(true);

	auto listener = EventListenerAcceleration::create([](Acceleration* acceleration, Event* event){
		log("X: %f; Y: %f; Z:%f; ", acceleration->x, acceleration->y, acceleration->z);
	});

	dispatcher->addEventListenerWithSceneGraphPriority(listener, this);
}

void HelloWorld::testKeyboardEvent()
{
	auto listener = EventListenerKeyboard::create();

	listener->onKeyPressed = [&](EventKeyboard::KeyCode code, Event* event){
		if (code == EventKeyboard::KeyCode::KEY_A)
		{
			log("onKeyPressed--A");
		}
	};

	listener->onKeyReleased = [&](EventKeyboard::KeyCode code, Event* event){
		if (code == EventKeyboard::KeyCode::KEY_A)
		{
			log("onKeyReleased--A");
		}
	};

	dispatcher->addEventListenerWithSceneGraphPriority(listener, this);
}

void HelloWorld::testMouseEvent()
{
	auto listener = EventListenerMouse::create();

	listener->onMouseDown = [&](Event* event){
		log("onMouseDown");
	};

	listener->onMouseMove = [&](Event* event){
		log("onMouseMove");
		EventMouse* e = (EventMouse*)event;
		touch_pos = e->getLocation();
		streak->setPosition(touch_pos);
	};

	listener->onMouseUp = [&](Event* event){
		log("onMouseUp");
	};

	listener->onMouseScroll = [&](Event* event){
		log("onMouseScroll");
	};

	dispatcher->addEventListenerWithSceneGraphPriority(listener, this);
}

void HelloWorld::testCustomEvent()
{
	auto listener = EventListenerCustom::create("ME_CUSTOM_EVENT_TEST", [](EventCustom* event){
		Vec2* pos = (Vec2*)(event->getUserData());
	});

	//注册自定义事件
	dispatcher->addEventListenerWithFixedPriority(listener, 12);
	//发布事件
	Director::getInstance()->getEventDispatcher()->dispatchCustomEvent("ME_CUSTOM_EVENT_TEST");
}
